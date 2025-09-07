using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FileHub.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileHub.API.Tests;

public class FileHubWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationContext>();

            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connection));

            services.AddAuthentication(defaultScheme: "TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", _ => { });
        });

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var options = new Dictionary<string, string?>
            {
                ["ContentDirectory"] = Path.GetTempPath()
            };

            config.AddInMemoryCollection(options);
        });
    }
}
