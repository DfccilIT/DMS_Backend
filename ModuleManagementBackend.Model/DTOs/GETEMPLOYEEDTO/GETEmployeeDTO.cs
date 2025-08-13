using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.Model.DTOs.GETEMPLOYEEDTO
{
    public class EmployeeProfileDto
    {
        public int EmpId { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string PostDescription { get; set; }
        public string Lavel { get; set; }
        public string PositionGrade { get; set; }
        public string ReDesignatedGrade { get; set; }
        public string UnitName { get; set; }
        public int UnitId { get; set; }
        public string EmpMobileNo { get; set; }
        public string PersonalMobile { get; set; }
        public string EmpEmail { get; set; }
        public string PersonalEmail { get; set; }
        public int? ManagerId { get; set; }
        public string ManagerCode { get; set; }
        public string ManagerName { get; set; }
        public int TotalReportingCount { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Gender { get; set; }
        public string ExtnNo { get; set; }
        public string FaxNo { get; set; }
        public string MTNNo { get; set; }
        public string TOemploy { get; set; }
        public string ImageURL { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? DOJ { get; set; }
        public DateTime? DOR { get; set; }
        public DateTime? AnniversaryDate { get; set; }
        public string ReportingOfficer { get; set; }
        public string ReportingOfficerName { get; set; }
        public int IsPRMSElegible { get; set; }
        public string AboutUs { get; set; }
        public string Category { get; set; }
        public string SpecifiedCategory { get; set; }
        public decimal BasicSalary { get; set; }
        public string PAN { get; set; }
        public bool IsEditProfileSubmitted { get; set; }
        public bool IsReportingOfficer { get; set; }
        public bool IsAllowedLoginForRetired { get; set; }
    }

    public class UnitDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Add other unit properties as needed
    }
}
