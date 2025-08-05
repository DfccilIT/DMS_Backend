using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.UserDTO
{
    public class DtoEditEmpRole
    {
        public int EmpCode { get; set; }
        public int EmpUnitId { get; set; }

        public List<DtoRole> Roles { get; set; } 
    }

    public class DtoRole
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
