using Microsoft.EntityFrameworkCore;
using Tweetbook.Data;

namespace Tweetbook.Installers;
public class DbInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        //builder.Services.AddDefaultIdentity<IdentityUser>()
        //    .AddEntityFrameworkStores<DataContext>();
    }
}
