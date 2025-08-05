using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.UserDTO
{
    public class AddUserRoleMapping
    {
        public int EmpCode { get; set; }

        public int EmpUnitId { get; set; }

        public List<userRole> UserRoles { get; set;}
    }

    public class userRole
    {
        public int RoleId { get; set; }
    }
}
