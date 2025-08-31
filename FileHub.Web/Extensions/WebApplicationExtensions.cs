using FileHub.Application.Abstractions.Data;
using FileHub.Data.Services;

namespace FileHub.Web.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCookiePolicy();
        app.UseCors("Web");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<IApplicationContext>();

        await context.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeederService>();

        await seeder.SeedAsync();

        return app;
    }
}
