using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.UserDTO
{
    public class DtoAddUserRole
    {
        public string? Name { get; set; }
        public string? Description { get; set;}
        public List<Permission>?PermissionIds { get; set; }
    }
    public class Permission
    {
        public int? PermissionId { get; set; }
    }
    public class DtoAddUserRoleWithoutPermission
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
