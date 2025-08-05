using Microsoft.EntityFrameworkCore;
using ModuleManagementBackend.DAL.DbEntities.UserEntities;


namespace ModuleManagementBackend.DAL.DBContext
{
    public class ModuleManagementDbContext : DbContext
    {
        public ModuleManagementDbContext(DbContextOptions<ModuleManagementDbContext> options) : base(options)
        {

        }

        #region User Table Defination 

        public DbSet<RoleMaster> RoleMasters { get; set; }
        public DbSet<UserRoleMapping> userRoleMappings { get; set; }
        public DbSet<RolePermissionMapping> RolePermissionMappings { get; set; }
        public DbSet<UserPermissionMaster> UserPermissionMasters { get; set; }
        #endregion

     


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
        }

    }
}
