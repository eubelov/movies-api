using Microsoft.ApplicationInsights.Extensibility;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace MoviesApi.Mvc.Extensions;

public class LoggingExtensions
{
    public static void ConfigureSerilog(HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration configuration)
    {
        var logConfig = configuration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("source", Environment.MachineName);

        logConfig.WriteTo.ApplicationInsights(serviceProvider.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Events);

        var consoleTemplate = context.Configuration["Logging:ConsoleOutputTemplate"];
        if (!string.IsNullOrEmpty(consoleTemplate))
        {
            logConfig.WriteTo.Console(
                outputTemplate: consoleTemplate,
                theme: AnsiConsoleTheme.Literate);
        }
    }
}