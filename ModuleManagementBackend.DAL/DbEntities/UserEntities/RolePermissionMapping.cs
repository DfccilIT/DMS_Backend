using System.ComponentModel.DataAnnotations.Schema;
namespace ModuleManagementBackend.DAL.DbEntities.UserEntities
{
    public class RolePermissionMapping:BaseEntity
    {
        [ForeignKey(nameof(Role_Master))]
        public int RoleMasterId { get; set; }

        [ForeignKey(nameof(Permission_Master))]
        public int PermissionMasterId { get; set; }

        public virtual RoleMaster? Role_Master { get; set; }
        public virtual UserPermissionMaster? Permission_Master { get; set; }
    }
}
