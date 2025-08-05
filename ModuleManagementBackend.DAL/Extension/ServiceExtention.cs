using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleManagementBackend.DAL.DBContext;
using ModuleManagementBackend.DAL.Models;
namespace ModuleManagementBackend.DAL.Extension
{
    public static class ServiceExtention
    {
        public static void AddDataAccessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
            var connectstring = "";
            var connectstringSap="";
            if (Environment == "DFCCIL")
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSProd");
                connectstringSap = configuration.GetConnectionString("SapTokenCSProd");
            }
            else if (Environment == "DFCCIL_UAT")
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSUat");
                connectstringSap = configuration.GetConnectionString("SapTokenCSUat");
            }
            else
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSLocal");
                connectstringSap = configuration.GetConnectionString("SapTokenCSLocal");
            }
            services.AddDbContext<ModuleManagementDbContext>(options =>
            {

                options.UseSqlServer(connectstring);
            });
            services.AddDbContext<SAPTOKENContext>(options =>
            {
                options.UseSqlServer(connectstringSap, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
            });
        }
    }
}
