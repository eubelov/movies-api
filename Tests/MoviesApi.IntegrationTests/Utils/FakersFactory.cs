using Bogus;

using MoviesApi.Features.GetMovies;

namespace MoviesApi.IntegrationTests.Utils;

public static class FakersFactory
{
    public static Faker<GetMoviesResponse.Movie> GetMovieFaker()
    {
        var faker = new Faker<GetMoviesResponse.Movie>();

        faker
            .RuleFor(x => x.Actors, x => x.Lorem.Sentence())
            .RuleFor(x => x.Awards, x => x.Lorem.Sentence())
            .RuleFor(x => x.Country, x => x.Address.Country())
            .RuleFor(x => x.Director, x => x.Lorem.Word())
            .RuleFor(x => x.Genre, x => x.Lorem.Word())
            .RuleFor(x => x.Language, x => x.Lorem.Word())
            .RuleFor(x => x.Plot, x => x.Lorem.Sentence())
            .RuleFor(x => x.Poster, x => x.Internet.Url())
            .RuleFor(x => x.Rated, x => x.Lorem.Word())
            .RuleFor(x => x.Released, x => x.Lorem.Word())
            .RuleFor(x => x.Runtime, x => x.Lorem.Word())
            .RuleFor(x => x.Type, x => x.Lorem.Word())
            .RuleFor(x => x.Votes, x => x.Lorem.Word())
            .RuleFor(x => x.Writer, x => x.Lorem.Word())
            .RuleFor(x => x.Price, x => x.Random.Decimal());

        return faker;
    }
}