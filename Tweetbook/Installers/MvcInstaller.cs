using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Tweetbook.Authorization;
using Tweetbook.Filters;
using Tweetbook.Options;
using Tweetbook.Services;

namespace Tweetbook.Installers;
public class MvcInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add<ValidationFilter>();
        })
            .AddFluentValidation(mvcConfiguration =>
                mvcConfiguration.RegisterValidatorsFromAssemblyContaining<Program>());

        var jwtSettings = new JwtSettings();
        configuration.Bind(nameof(jwtSettings), jwtSettings);

        services.AddScoped<IIdentityService, IdentityService>();

        services.AddSingleton<IUriService>(provider =>
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();
            var request = accessor.HttpContext.Request;
            var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
            return new UriService(absoluteUri);
        });

        services.AddSingleton(jwtSettings);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = false,
            ValidateLifetime = true,
        };

        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParameters;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("MustWorkForKinawy", policy =>
            {
                policy.AddRequirements(new WorksForCompanyRequirement("kinawy.com"));
            });
        });

        services.AddSingleton<IAuthorizationHandler, WorksForCompanyHandler>();

    }
}
