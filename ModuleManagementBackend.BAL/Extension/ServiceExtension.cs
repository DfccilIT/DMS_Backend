using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.BAL.IServices.ICacheServices;
using ModuleManagementBackend.BAL.Services;
using ModuleManagementBackend.BAL.Services.CacheServices;
using ModuleManagementBackend.DAL.DapperServices;

namespace ModuleManagementBackend.BAL.Extension
{
    public static class ServiceExtension
    {
        public static void AddBusinessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<IDatabaseChangesService, DatabaseChangesService>();
           
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IDapperService, DapperService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IModuleManagementService, ModuleManagementService>();
            services.AddScoped<IPolicyService, PoliciesService>();
            services.AddTransient<INotificationManageService, NotificationManageService>();
            

        }
    }
}
