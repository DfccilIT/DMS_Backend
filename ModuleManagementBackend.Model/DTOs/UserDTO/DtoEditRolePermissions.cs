using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.UserDTO
{
    public class DtoEditRolePermissions
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<DtoPermission> PermissionIds { get; set; }
    }

}
