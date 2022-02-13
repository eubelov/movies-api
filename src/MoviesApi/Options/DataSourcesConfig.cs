namespace MoviesApi.Options;

public class DataSourcesConfig
{
    public DataSource[] DataSources { get; init; } = Array.Empty<DataSource>();

    public class DataSource
    {
        public string Name { get; init; } = string.Empty;

        public string BaseUrl { get; init; } = string.Empty;

        public string ListRoute { get; init; } = string.Empty;

        public string DetailsRoute { get; init; } = string.Empty;

        public int MaxConcurrency { get; init; }

        public TimeSpan Timeout { get; init; }

        public TimeSpan CacheTtl { get; init; }

        public string AccessToken { get; init; } = string.Empty;
    }
}