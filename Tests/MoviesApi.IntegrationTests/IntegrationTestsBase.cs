using System.Net;
using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using WireMock.Server;

using Xunit;

namespace MoviesApi.IntegrationTests;

public abstract class IntegrationTestsBase : IClassFixture<CustomWebApplicationFactory<Startup>>
{
    protected IntegrationTestsBase(CustomWebApplicationFactory<Startup> factory)
    {
        this.Factory = factory;
        this.WireMockServer = factory.Services.GetRequiredService<WireMockServer>();
    }

    protected CustomWebApplicationFactory<Startup> Factory { get; }

    protected WireMockServer WireMockServer { get; }

    protected HttpClient CreateClient()
    {
        var client = this.Factory.CreateClient();
        return client;
    }

    protected Task<HttpCallResult<T>> Get<T>(string route)
    {
        var client = this.CreateClient();

        return this.Get<T>(client, route);
    }

    protected async Task<HttpCallResult<T>> Get<T>(HttpClient client, string route)
    {
        var result = await client.GetAsync(route);
        var response = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());

        return new(response, result.StatusCode)
        {
            Headers = result.Headers,
        };
    }

    public record HttpCallResult<T>(T? Data, HttpStatusCode StatusCode)
    {
        public HttpResponseHeaders? Headers { init; get; }
    }
}