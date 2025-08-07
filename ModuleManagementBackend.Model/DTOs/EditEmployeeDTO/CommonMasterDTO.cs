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

    public class NoticeBoardDto
    {
        public string Msg { get; set; }
        public string Doc { get; set; }
        public int? Status { get; set; }
        public string CreateBy { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
    }

    public class AddDependentDto
    {
        public int pkDependentId { get; set; }       
        public string EmployeeCode { get; set; }     
        public string Relation { get; set; }
        public string DName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public int? status { get; set; }
    }
}
