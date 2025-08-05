using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.UserDTO
{
    public class UserDTO
    {
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string UserType { get; set; }
        public int? FloorWiseBoxId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ExtensionNo { get; set; }
        public int? PositionId { get; set; }
    }
}
