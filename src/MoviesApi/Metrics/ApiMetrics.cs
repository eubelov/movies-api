using Prometheus;

namespace MoviesApi.Metrics;

public static class ApiMetrics
{
    public const string Prefix = "movies_api";

    public static readonly Counter RetriedHttpRequestsCount
        = Prometheus.Metrics.CreateCounter($"{Prefix}_http_retried_requests_count", "Number of retried HTTP requests", "source");

    public static readonly Counter HttpTimeoutsCount
        = Prometheus.Metrics.CreateCounter($"{Prefix}_http_timeouts_count", "Number of timed out HTTP requests", "source");
}