using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.EditEmployeeDTO
{
    public class AprooveEmployeeReportDto
    {
        public string EmployeeCode { get; set; }
        public bool IsApproved { get; set; }
        public string Remarks { get; set; }
    }
}
