using FileHub.Application.Extensions;
using FileHub.Data.Extensions;
using FileHub.Storage.Extensions;
using FileHub.Utilities.Extensions;
using FileHub.Web.Extensions;

namespace FileHub.Web;

class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureConfiguration(builder);
        ConfigureServices(builder);
        ConfigureKestrel(builder);

        var app = builder.Build();

        await app.InitializeDatabaseAsync().ConfigureAwait(false);

        app.ConfigureMiddleware();

        await app.RunAsync();
    }

    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddData(builder.Configuration)
            .AddIdentity()
            .AddCustomAuthentication()
            .AddCustomCors(builder.Configuration);

        builder.Services
            .AddStorage(builder.Configuration)
            .AddWebServices(builder.Configuration)
            .AddUtilities(builder.Configuration)
            .AddApplication();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddHttpContextAccessor();
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = builder.Configuration.GetValue<long?>("MaxRequestBodySizeBytes");
        });
    }
}
