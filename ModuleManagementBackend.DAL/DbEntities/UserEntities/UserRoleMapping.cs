using System.ComponentModel.DataAnnotations.Schema;

namespace ModuleManagementBackend.DAL.DbEntities.UserEntities
{
    public class UserRoleMapping:BaseEntity
    {
        public int? EmpCode { get; set; }

        //public string? Unit { get; set; }
        public int UnitId { get; set; }

        public string? UnitCode { get; set; }

        [ForeignKey(nameof(Role))]
        public int? RoleMasterId { get; set; } 
        public virtual RoleMaster? Role { get; set; }


    }

}
