using System.Security.Claims;
using Api.Data;
using Api.Data.Entities;
using Api.Factories;
using Api.Middleware;
using Api.Models.Common;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api;

class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorizationBuilder();

        ConfigureDatabase(builder);
        ConfigureIdentity(builder);
        ConfigureAuthentication(builder);
        ConfigureCors(builder);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument();

        RegisterServices(builder);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = null;
        });

        var app = builder.Build();

        MigrateDatabase(app);

        ConfigureMiddleware(app, builder.Environment);

        app.Run();
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection")
            )
        );
    }

    private static void ConfigureIdentity(WebApplicationBuilder builder)
    {
        builder
            .Services.AddIdentityCore<User>()
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        });
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies(options =>
            {
                options.ApplicationCookie?.Configure(config =>
                {
                    config.Cookie.Name = "FileHub";
                    config.ExpireTimeSpan = TimeSpan.FromDays(30);
                    config.SlidingExpiration = true;
                    config.LoginPath = "/account/login";
                    config.LogoutPath = "/account/logout";
                    config.Cookie.SameSite = SameSiteMode.Lax;

                    config.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            context.Response.StatusCode =
                                StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/text";
                            return context.Response.WriteAsync(Messages.NotLoggedIn);
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode =
                                StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        },
                    };
                });
            });
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        var url = builder.Configuration.GetValue<string>("WebUrl");

        builder.Services.AddCors(options =>
            options.AddPolicy(
                "SvelteFrontend",
                policy =>
                    policy
                        .WithOrigins(url!)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            )
        );
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IStorageService, StorageService>();
        builder.Services.AddScoped<IFileRepository, FileRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<ITagService, TagService>();
        builder.Services.AddScoped<ResponseFactory>();
    }

    private static void MigrateDatabase(WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var context =
            serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        // app.MapCustomIdentityApi<User>(app.Configuration);

        app.UseMiddleware<ExceptionHandler>();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors("SvelteFrontend");

        // Enable authentication and authorization after CORS Middleware
        // processing (UseCors) in case the Authorization Middleware tries
        // to initiate a challenge before the CORS Middleware has a chance
        // to set the appropriate headers.
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();

        app.MapControllers();
    }
}
