using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace Tweetbook.Installers;
public class SwaggerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Tweetbook API",
                Version = "v1"
            });

            x.ExampleFilters();

            var security = new Dictionary<string, IEnumerable<string>>
            {
                { "Bearer", new string[0] }
            };

            x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the bearer schema",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            x.IncludeXmlComments(xmlPath);

            //x.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {new OpenApiSecurityScheme{Reference = new OpenApiReference
            //    {
            //        Id = "Bearer",
            //        Type = ReferenceType.Schema
            //    }
            //    }, new List<string>() }
            //});

        });

        services.AddSwaggerExamplesFromAssemblyOf<Program>();

    }
}
