using System.Net;

using MoviesApi.Features.GetMovies;
using MoviesApi.IntegrationTests.Utils;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

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
        var cinemaworldIds = new
        {
            Movies = new[]
            {
                new { ID = "1" },
                new { ID = "2" },
                new { ID = "3" },
            },
        };

        var filmworldIds = new
        {
            Movies = new[]
            {
                new { ID = "123" },
                new { ID = "456" },
            },
        };

        var movieFaker = FakersFactory.GetMovieFaker();

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/cinemaworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(cinemaworldIds)
                    .WithStatusCode(HttpStatusCode.OK));

        foreach (var id in cinemaworldIds.Movies)
        {
            this.WireMockServer.Given(
                    Request.Create()
                        .WithPath(r => r.StartsWith($"/api/cinemaworld/movie/{id.ID}"))
                        .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(movieFaker.RuleFor(x => x.ID, _ => id.ID).Generate())
                        .WithStatusCode(HttpStatusCode.OK));
        }

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/filmworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(filmworldIds)
                    .WithStatusCode(HttpStatusCode.OK));

        foreach (var id in filmworldIds.Movies)
        {
            this.WireMockServer.Given(
                    Request.Create()
                        .WithPath(r => r.StartsWith($"/api/filmworld/movie/{id.ID}"))
                        .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(movieFaker.RuleFor(x => x.ID, _ => id.ID).Generate())
                        .WithStatusCode(HttpStatusCode.OK));
        }

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.NotEmpty(getMoviesResponse!.Movies);
    }

    [Fact]
    public async Task RetriesFailedRequests()
    {
        var cinemaworldIds = new
        {
            Movies = new[]
            {
                new { ID = "1" },
                new { ID = "2" },
                new { ID = "3" },
            },
        };

        var filmworldIds = new
        {
            Movies = new[]
            {
                new { ID = "123" },
                new { ID = "456" },
            },
        };

        var movieFaker = FakersFactory.GetMovieFaker();

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/cinemaworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(cinemaworldIds)
                    .WithRandomDelay(1000, 4500)
                    .WithStatusCode(HttpStatusCode.OK));

        foreach (var id in cinemaworldIds.Movies)
        {
            this.WireMockServer.Given(
                    Request.Create()
                        .WithPath(r => r.StartsWith($"/api/cinemaworld/movie/{id.ID}"))
                        .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(movieFaker.RuleFor(x => x.ID, _ => id.ID).Generate())
                        .WithStatusCode(HttpStatusCode.OK));
        }

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/filmworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(filmworldIds)
                    .WithRandomDelay(1000, 2800)
                    .WithStatusCode(HttpStatusCode.OK));

        foreach (var id in filmworldIds.Movies)
        {
            this.WireMockServer.Given(
                    Request.Create()
                        .WithPath(r => r.StartsWith($"/api/filmworld/movie/{id.ID}"))
                        .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(movieFaker.RuleFor(x => x.ID, _ => id.ID).Generate())
                        .WithStatusCode(HttpStatusCode.OK));
        }

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.NotEmpty(getMoviesResponse!.Movies);
    }

    [Fact]
    public async Task ReturnsListOfMoviesWhenOneSourceTimesOut()
    {
        var cinemaworldIds = new
        {
            Movies = new[]
            {
                new { ID = "1" },
                new { ID = "2" },
                new { ID = "3" },
            },
        };

        var filmworldIds = new
        {
            Movies = new[]
            {
                new { ID = "123" },
                new { ID = "456" },
            },
        };

        var movieFaker = FakersFactory.GetMovieFaker();

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/cinemaworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(cinemaworldIds)
                    .WithDelay(TimeSpan.FromSeconds(10))
                    .WithStatusCode(HttpStatusCode.OK));

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/filmworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(filmworldIds)
                    .WithStatusCode(HttpStatusCode.OK));

        foreach (var id in filmworldIds.Movies)
        {
            this.WireMockServer.Given(
                    Request.Create()
                        .WithPath(r => r.StartsWith($"/api/filmworld/movie/{id.ID}"))
                        .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(movieFaker.RuleFor(x => x.ID, _ => id.ID).Generate())
                        .WithStatusCode(HttpStatusCode.OK));
        }

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.NotEmpty(getMoviesResponse!.Movies);
    }

    [Fact]
    public async Task ReturnsEmptyListOfMoviesWhenAllSourceTimesOut()
    {
        var cinemaworldIds = new
        {
            Movies = new[]
            {
                new { ID = "1" },
                new { ID = "2" },
                new { ID = "3" },
            },
        };

        var filmworldIds = new
        {
            Movies = new[]
            {
                new { ID = "123" },
                new { ID = "456" },
            },
        };

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/cinemaworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(cinemaworldIds)
                    .WithDelay(TimeSpan.FromSeconds(10))
                    .WithStatusCode(HttpStatusCode.OK));

        this.WireMockServer.Given(
                Request.Create()
                    .WithPath("/api/filmworld/movies")
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithBodyAsJson(filmworldIds)
                    .WithDelay(TimeSpan.FromSeconds(10))
                    .WithStatusCode(HttpStatusCode.OK));

        var (getMoviesResponse, httpStatusCode) = await this.Get<GetMoviesResponse>(Constants.Routes.Movies.Get);
        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
        Assert.Empty(getMoviesResponse!.Movies);
    }
}