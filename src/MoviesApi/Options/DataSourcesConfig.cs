namespace MoviesApi.Options;

public class DataSourcesConfig
{
    public DataSource[] DataSources { get; set; } = Array.Empty<DataSource>();

    public class DataSource
    {
        public string Name { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = string.Empty;

        public string ListRoute { get; set; } = string.Empty;

        public string DetailsRoute { get; set; } = string.Empty;

        public int MaxConcurrency { get; set; }

        public TimeSpan Timeout { get; set; }

        public TimeSpan CacheTtl { get; set; }

        public string AccessToken { get; set; } = string.Empty;
    }
}