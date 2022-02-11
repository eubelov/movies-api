using System.Reflection;

using CorrelationId;
using CorrelationId.DependencyInjection;

using MediatR;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

using MoviesApi.Features.GetMovies;
using MoviesApi.Metrics;
using MoviesApi.Mvc.Extensions;
using MoviesApi.Mvc.Filters;
using MoviesApi.Options;
using MoviesApi.RequestsPipeline;

using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;

using Prometheus;

using Serilog;

namespace MoviesApi;

public class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IAsyncCacheProvider cacheProvider,
        IPolicyRegistry<string> registry,
        IOptions<DataSourcesConfig> dataSourceOptions)
    {
        foreach (var dataSource in dataSourceOptions.Value.DataSources)
        {
            registry.Add(
                $"ListCachingPolicy@{dataSource.Name}",
                Policy.CacheAsync<GetMoviesListResponse>(cacheProvider, dataSource.CacheTtl));

            registry.Add(
                $"ItemCachingPolicy@{dataSource.Name}",
                Policy.CacheAsync<GetMoviesResponse.Movie?>(cacheProvider, dataSource.CacheTtl));

            registry.Add(
                $"ItemsBulkhead@{dataSource.Name}",
                Policy.BulkheadAsync<GetMoviesResponse.Movie?>(dataSource.MaxConcurrency, int.MaxValue));
        }

        app.UseCorrelationId();
        app.UseSerilogRequestLogging();
        app.UseMetricServer();
        app.UseHttpMetrics();
        app.UseRouting();
        app.UseCors("AllowAll");

        app.UseSwagger();
        app.UseSwaggerUI(
            c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Movies API"); });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseEndpoints(
            endpoints => { endpoints.MapControllers(); });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddMediatR(typeof(Startup).Assembly);
        services.AddMemoryCache();
        services.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();
        services.AddPolicyRegistry();
        services.AddOptions();

        services.Configure<DataSourcesConfig>(this.configuration.GetSection(nameof(DataSourcesConfig)));
        services.Configure<TelemetryConfiguration>(
            config =>
                {
                    config.TelemetryChannel = new InMemoryChannel();
                });

        services.AddApiVersioning(
            options =>
                {
                    options.ReportApiVersions = false;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new(1, 0);
                });

        services.AddDefaultCorrelationId(
            x =>
                {
                    x.RequestHeader = "X-CorrelationId";
                    x.AddToLoggingScope = true;
                    x.IncludeInResponse = true;
                });

        services.AddCors(
            options =>
                {
                    options.AddPolicy(
                        "AllowAll",
                        builder =>
                            {
                                builder.AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .SetIsOriginAllowed(_ => true)
                                    .AllowCredentials();
                            });
                });

        services.AddControllers(
                options =>
                    {
                        options.UseGlobalRoutePrefix("api/v{version:apiVersion}");
                        options.Filters.Add<HttpResponseExceptionFilter>();
                    })
            .AddNewtonsoftJson();

        services.AddSwaggerGen(
            c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new()
                        {
                            Title = "Movies API",
                            Version = "v1",
                        });

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                    c.DocInclusionPredicate((_, _) => true);
                });

        services
            .AddScoped<HttpResponseExceptionFilter>();

        services
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

#pragma warning disable ASP0000
        var sp = services.BuildServiceProvider();
#pragma warning restore ASP0000

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var dataSources = sp.GetRequiredService<IOptions<DataSourcesConfig>>();

        foreach (var dataSource in dataSources.Value.DataSources)
        {
            services.AddHttpClient(
                    dataSource.Name,
                    x =>
                        {
                            x.BaseAddress = new(dataSource.BaseUrl);
                            x.DefaultRequestHeaders.Add("x-access-token", dataSource.AccessToken);
                        })
                .AddPolicyHandler(GetRetryPolicy(dataSource.Name, loggerFactory))
                .AddPolicyHandler(GetTimeoutPolicy(dataSource.Name, dataSource.Timeout, loggerFactory));
        }
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string source, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger($"retries@{source}");

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                },
                (result, span) =>
                    {
                        ApiMetrics.RetriedHttpRequestsCount.Labels(source).Inc();
                        logger.LogWarning(result.Exception, $"HTTP status code: {result.Result?.StatusCode}. Retrying in {span}");
                    });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(string source, TimeSpan timeout, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger($"timeouts@{source}");

        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout,
            (_, value, _, _) =>
                {
                    ApiMetrics.HttpTimeoutsCount.Labels(source).Inc();
                    logger.LogWarning($"Operation timed out after {value}");
                    return Task.CompletedTask;
                });
    }
}