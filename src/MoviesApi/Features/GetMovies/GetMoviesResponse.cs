namespace MoviesApi.Features.GetMovies;

public sealed class GetMoviesResponse
{
    public Movie[] Movies { get; init; } = Array.Empty<Movie>();

    public sealed class Movie
    {
        public string Title { get; init; } = string.Empty;

        public int Year { get; init; }

        public string Rated { get; init; } = string.Empty;

        public string Released { get; init; } = string.Empty;

        public string Runtime { get; init; } = string.Empty;

        public string Genre { get; init; } = string.Empty;

        public string Director { get; init; } = string.Empty;

        public string Writer { get; init; } = string.Empty;

        public string Actors { get; init; } = string.Empty;

        public string Plot { get; init; } = string.Empty;

        public string Language { get; init; } = string.Empty;

        public string Country { get; init; } = string.Empty;

        public string Awards { get; init; } = string.Empty;

        public string Poster { get; init; } = string.Empty;

        public int Metascore { get; init; }

        public double Rating { get; init; }

        public string Votes { get; init; } = string.Empty;

        public string ID { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public decimal Price { get; init; }
    }
}