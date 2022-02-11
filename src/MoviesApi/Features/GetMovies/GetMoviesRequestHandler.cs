using MediatR;

using Microsoft.Extensions.Options;

using MoviesApi.Models;
using MoviesApi.Options;

using Polly;
using Polly.Bulkhead;
using Polly.Caching;
using Polly.Registry;

namespace MoviesApi.Features.GetMovies;

internal sealed class GetMoviesRequestHandler : IRequestHandler<GetMoviesRequest, MediatorResponse<GetMoviesResponse>>
{
    private readonly IHttpClientFactory httpClientFactory;

    private readonly IPolicyRegistry<string> policyRegistry;

    private readonly ILogger<GetMoviesRequestHandler> logger;

    private readonly DataSourcesConfig options;

    public GetMoviesRequestHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<DataSourcesConfig> options,
        IPolicyRegistry<string> policyRegistry,
        ILogger<GetMoviesRequestHandler> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.policyRegistry = policyRegistry;
        this.logger = logger;
        this.options = options.Value;
    }

    public async Task<MediatorResponse<GetMoviesResponse>> Handle(GetMoviesRequest request, CancellationToken token)
    {
        var loadTasks = this.options.DataSources.Select(x => this.LoadInternal(x, token));
        var allMovies = await Task.WhenAll(loadTasks);

        return new()
        {
            Result = new()
            {
                Movies = allMovies.Aggregate(
                        Array.Empty<GetMoviesResponse.Movie>(),
                        (a, b) => a.Union(b).ToArray())
                    .GroupBy(x => x.Title)
                    .Select(x => x.Count() == 1 ? x.Single() : x.MinBy(m => m.Price)!)
                    .ToArray(),
            },
        };
    }

    private async Task<GetMoviesResponse.Movie[]> LoadInternal(DataSourcesConfig.DataSource dataSource, CancellationToken token)
    {
        var client = this.httpClientFactory.CreateClient(dataSource.Name);
        var moviesList = await this.LoadMoviesList(client, dataSource.Name, dataSource.ListRoute, token);

        var bulkheadPolicy = this.policyRegistry.Get<AsyncBulkheadPolicy<GetMoviesResponse.Movie?>>($"ItemsBulkhead@{dataSource.Name}");
        var loadMoviesTasks = moviesList.Movies.Select(
            x => bulkheadPolicy.ExecuteAsync(
                () => this.LoadMovie(client, dataSource.Name, x.ID, dataSource.DetailsRoute, token)));

        var movies = await Task.WhenAll(loadMoviesTasks);
        return movies.Where(x => x != null).ToArray()!;
    }

    private async Task<GetMoviesResponse.Movie?> LoadMovie(
        HttpClient client,
        string clientName,
        string movieId,
        string movieInfoUrl,
        CancellationToken token)
    {
        var cachePolicy = this.policyRegistry.Get<AsyncCachePolicy<GetMoviesResponse.Movie?>>($"ItemCachingPolicy@{clientName}");

        var result = await cachePolicy.ExecuteAndCaptureAsync(
                         _ => client.GetFromJsonAsync<GetMoviesResponse.Movie>(string.Format(movieInfoUrl, movieId), token),
                         new Context($"movie_info@{movieId}"));

        if (result.Outcome is OutcomeType.Failure)
        {
            this.logger.LogWarning(result.FinalException, $"Failed to get movie {movieId} from data source {clientName}");
        }

        return result.Result;
    }

    private async Task<GetMoviesListResponse> LoadMoviesList(HttpClient client, string clientName, string listUrl, CancellationToken token)
    {
        var cachePolicy = this.policyRegistry.Get<AsyncCachePolicy<GetMoviesListResponse>>($"ListCachingPolicy@{clientName}");

        var result = await cachePolicy.ExecuteAndCaptureAsync(
                         _ => client.GetFromJsonAsync<GetMoviesListResponse>(listUrl, token)!,
                         new Context($"movies_list@{clientName}"));

        if (result.Outcome is OutcomeType.Failure)
        {
            this.logger.LogWarning(result.FinalException, $"Failed to get movies list from data source {clientName}");
            return new();
        }

        return result.Result;
    }
}