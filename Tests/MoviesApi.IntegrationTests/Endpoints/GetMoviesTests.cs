using System.Net;

using MoviesApi.Features.GetMovies;
using MoviesApi.IntegrationTests.Utils;

using Xunit;

namespace MoviesApi.IntegrationTests.Endpoints;

public class GetMoviesTests : IntegrationTestsBase
{
    public GetMoviesTests(CustomWebApplicationFactory<Startup> factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ReturnsListOfMovies()
    {
        var titles = new[] { "Title 1", "Title 2", "Title 3" };
        DataSourceExtensions.SetupSource(this.WireMockServer, "cinemaworld",  new[] { "1", "2", "3" }, titles);
        DataSourceExtensions.SetupSource(this.WireMockServer, "filmworld", new[] { "ds_2_1", "ds_2_2" }, titles);

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.NotEmpty(getMoviesResponse!.Movies);
    }

    [Fact]
    public async Task ReturnsListOfMoviesWhenOneSourceTimesOut()
    {
        var titles = new[] { "Title 1", "Title 2", "Title 3" };
        var expectedIds = new[] { "ds_2_1", "ds_2_2" };
        DataSourceExtensions.SetupSource(this.WireMockServer, "cinemaworld",  new[] { "1", "2", "3" }, titles, 10);
        DataSourceExtensions.SetupSource(this.WireMockServer, "filmworld", expectedIds, titles);

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.True(expectedIds.SequenceEqual(getMoviesResponse!.Movies.Select(x => x.ID).OrderBy(x => x)));
    }

    [Fact]
    public async Task ReturnsEmptyListOfMoviesWhenAllSourceTimesOut()
    {
        var titles = new[] { "Title 1", "Title 2", "Title 3" };
        DataSourceExtensions.SetupSource(this.WireMockServer, "cinemaworld",  new[] { "1", "2", "3" }, titles, 10);
        DataSourceExtensions.SetupSource(this.WireMockServer, "filmworld", new[] { "ds_2_1", "ds_2_2" }, titles, 10);

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.Empty(getMoviesResponse!.Movies);
    }
}