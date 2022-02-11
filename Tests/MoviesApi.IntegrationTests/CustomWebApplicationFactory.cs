using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using WireMock.Server;

namespace MoviesApi.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var ws = WireMockServer.Start();

        builder.UseEnvironment("SUT");
        builder.ConfigureAppConfiguration(
            c =>
                {
                    c.AddConfiguration(
                        new ConfigurationBuilder()
                            .AddJsonFile("appsettings.sut.json")
                            .AddUserSecrets<CustomWebApplicationFactory<TStartup>>()
                            .AddInMemoryCollection(
                                new KeyValuePair<string, string>[]
                                {
                                    new("DataSourcesConfig:DataSources:0:BaseUrl", ws.Urls[0]),
                                    new("DataSourcesConfig:DataSources:1:BaseUrl", ws.Urls[0]),
                                })
                            .Build());
                });

        builder.ConfigureServices(
            c => { c.AddSingleton(ws); });
    }
}