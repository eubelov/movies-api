using System.Net;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MoviesApi.IntegrationTests.Utils;

public class DataSourceExtensions
{
    public static void SetupSource(WireMockServer server, string name, string[] ids, string[] titles, int delaySeconds = 0)
    {
        var movieFaker = FakersFactory.GetMovieFaker();
        var response = new
        {
            Movies = ids.Select(x => new { ID = x }),
        };

        var resp = Response.Create();
        if (delaySeconds != 0)
        {
            resp = resp.WithDelay(TimeSpan.FromSeconds(delaySeconds));
        }

        server.Given(
                Request.Create()
                    .WithPath($"/api/{name}/movies")
                    .UsingGet())
            .RespondWith(
                resp
                    .WithBodyAsJson(response)
                    .WithStatusCode(HttpStatusCode.OK));

        var index = 0;
        foreach (var id in ids)
        {
            server.Given(
                    Request.Create()
                        .WithPath(r => r.StartsWith($"/api/{name}/movie/{id}"))
                        .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithBodyAsJson(
                            movieFaker
                                .RuleFor(x => x.ID, _ => id)
                                .RuleFor(x => x.Title, _ => titles[index++ % titles.Length])
                                .Generate())
                        .WithStatusCode(HttpStatusCode.OK));
        }
    }
}