using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.UserDTO
{
    public class DtoEditUserRolesByEmpCode
    {
        public int EmpCode { get; set; } // Employee code for whom roles and permissions are being updated
        public List<DtoRoleWithPermissions> Roles { get; set; } // List of roles with their permissions
    }

    public class DtoRoleWithPermissions
    {
        public int RoleId { get; set; } 
        public string? RoleName { get; set; } 
        public List<DtoPermission> PermissionIds { get; set; } 
    }

    public class DtoPermission
    {
        public int PermissionId { get; set; } // Permission identifier
        public string? PermissionName { get; set; } // Optional: For better readability or validation
    }


}
