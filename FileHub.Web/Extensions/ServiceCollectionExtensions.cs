using FileHub.Application.Abstractions.Services;
using FileHub.Data.Services;
using FileHub.Web.Configuration;
using FileHub.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace FileHub.Web.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = _ => false;
            options.MinimumSameSitePolicy = SameSiteMode.None;
            options.Secure = CookieSecurePolicy.Always;
        });

        services
            .AddAuthentication(options =>
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
                    config.Cookie.SameSite = SameSiteMode.None;
                    config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    config.Cookie.IsEssential = true;
                    config.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/text";

                            return context.Response.WriteAsync("Not logged in");
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }
                    };
                });
            });

        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsAllowedOrigins = configuration.GetValue<string>("CorsAllowedOrigins");

        if (string.IsNullOrWhiteSpace(corsAllowedOrigins))
            throw new InvalidOperationException("CorsOrigins is not set in configuration");

        var allowedOrigins = corsAllowedOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).ToArray();

        services.AddCors(options =>
            options.AddPolicy("Web", policy =>
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            )
        );

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FileHub",
                Version = "v0.1"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                x.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<WebOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<WebOptions>, WebOptionsValidator>();

        services.AddScoped<IFileLocationService, FileLocationService>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
