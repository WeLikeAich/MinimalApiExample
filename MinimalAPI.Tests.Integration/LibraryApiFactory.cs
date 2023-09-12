using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalAPI.Data;

namespace MinimalAPI.Tests.Integration;

public class LibraryApiFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(collection =>
        {
            collection.RemoveAll(typeof(IDbConnectionFactory));
            collection.TryAddSingleton<IDbConnectionFactory>(_ =>
               new SqliteConnectionFactory("DataSource=file:inmem?mode=memory&cache=shared"));
        });
    }
}