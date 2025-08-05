using Microsoft.AspNetCore.Http;
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


    public class UpdateEmployeeDto
    {
        public string EmployeeCode { get; set; }
        public string? PersonalMobile { get; set; }
        public string? ExtnNo { get; set; }
        public string? UpdatedBy { get; set; }
      
    }

    public class EmployeeOfTheMonthDto
    {
        public string EmpCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string? CreatedBy { get; set; }
        public IFormFile? photo { get; set; } 
    }
}
