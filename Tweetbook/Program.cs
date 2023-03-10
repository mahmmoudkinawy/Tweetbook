using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Tweetbook.Contracts.HealthChecks;
using Tweetbook.Data;
using Tweetbook.Installers;
using Tweetbook.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.InstallServicesInAssembly(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var swaggerOptions = new SwaggerOptions();
builder.Configuration.GetSection(nameof(swaggerOptions)).Bind(swaggerOptions);

app.UseSwagger(options =>
{
    options.RouteTemplate = swaggerOptions.JsonRoute;
});

app.UseSwaggerUI(option =>
{
    option.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Description);
});

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(x => new HealthCheck
            {
                Component = x.Key,
                Status = x.Value.Status.ToString(),
                Descirption = x.Value.Description
            }),
            Duration = report.TotalDuration
        };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
try
{
    await dbContext.Database.MigrateAsync();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var adminRole = new IdentityRole
        {
            Name = "Admin"
        };
        await roleManager.CreateAsync(adminRole);
    }

    if (!await roleManager.RoleExistsAsync("Poster"))
    {
        var adminRole = new IdentityRole
        {
            Name = "Poster"
        };
        await roleManager.CreateAsync(adminRole);
    }

}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured while applying migrations");
}

app.Run();
public partial class Program { }