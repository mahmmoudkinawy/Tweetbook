using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tweetbook.Data;
using Tweetbook.Services;

namespace Tweetbook.Installers;
public class DbInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<DataContext>();

        services.AddScoped<IPostService, PostService>();

    }
}
