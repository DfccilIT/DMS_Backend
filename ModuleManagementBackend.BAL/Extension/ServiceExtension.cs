using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.BAL.Services;
using ModuleManagementBackend.DAL.DapperServices;

namespace ModuleManagementBackend.BAL.Extension
{
    public static class ServiceExtension
    {
        public static void AddBusinessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IDapperService, DapperService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IModuleManagementService, ModuleManagementService>();
            

        }
    }
}
