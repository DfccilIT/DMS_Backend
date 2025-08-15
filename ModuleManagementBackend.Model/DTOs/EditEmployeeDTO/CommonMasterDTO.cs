using Microsoft.AspNetCore.Http;
using ModuleManagementBackend.Model.Common;
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

    public class AprooveContractualEmployeeDto
    {
        public int ContraualEmployeeRequestId { get; set; }
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
        public IFormFile Doc { get; set; }
        public string CreateBy { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
    }


    
    public class AddDependentDto
    {
        public string EmployeeCode { get; set; }     
        public string Relation { get; set; }
        public string DName { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public List<DependtentsDocuments>? DocumentFiles { get; set; }
    }

    public class  DependtentsDocuments
    {
        public IFormFile? DocumentFile { get; set; }
        public string? DocumentType { get; set; }
        public string? Remarks { get; set; }
    }

    public class ProfileCadre
    {
        public long EmployeeAutoId { get; set; }
        public string Empcode { get; set; }

        public string DeptName { get; set; }

        public string PositionGrade { get; set; }
    }
    public class SMSLogDetailDto
    {
        public int SMSSentId { get; set; }
        public string MobileNumber { get; set; }
        public string SMSText { get; set; }
        public DateTime SentOn { get; set; }
        public string UserId { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SMSLogRequest
    {
        public string EmpCode { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SearchText { get; set; }
    }

    public class PagedResponseModel : ResponseModel
    {
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
    }

}
