using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleManagementBackend.DAL.DBContext;
using ModuleManagementBackend.DAL.FirebaseModels;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.DAL.UpastithiModel;
namespace ModuleManagementBackend.DAL.Extension
{
    public static class ServiceExtention
    {
        public static void AddDataAccessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            var Environment = configuration["DeploymentModes"]?.ToString().Trim();
            var connectstring = "";
            var connectstringSap="";
            var connectstringVMS="";
            var connectstringUP="";
            if (Environment == "DFCCIL")
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSProd");
                connectstringSap = configuration.GetConnectionString("SapTokenCSProd");
                connectstringVMS = configuration.GetConnectionString("VMSCSProd");
                connectstringUP = configuration.GetConnectionString("UPCSProd");
            }
            else if (Environment == "DFCCIL_UAT")
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSUat");
                connectstringSap = configuration.GetConnectionString("SapTokenCSUat");
                connectstringVMS = configuration.GetConnectionString("VMSCSUat");
                connectstringUP = configuration.GetConnectionString("UPCSUat");
            }
            else
            {
                connectstring = configuration.GetConnectionString("ModuleManagementCSLocal");
                connectstringSap = configuration.GetConnectionString("SapTokenCSLocal");
                connectstringVMS = configuration.GetConnectionString("VMSCSLocal");
                connectstringUP = configuration.GetConnectionString("UPCSLocal");
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
            services.AddDbContext<UpastithiContext>(options =>
            {
                options.UseSqlServer(connectstringVMS, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
            });
            services.AddDbContext<UpastithiDBContext>(options =>
            {
                options.UseSqlServer(connectstringUP, sqlOptions =>
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
