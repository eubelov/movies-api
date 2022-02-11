namespace MoviesApi.Features.GetMovies;

public sealed class GetMoviesListResponse
{
    public Movie[] Movies { get; init; } = Array.Empty<Movie>();

    public sealed class Movie
    {
        public string ID { get; init; } = string.Empty;
    }
}