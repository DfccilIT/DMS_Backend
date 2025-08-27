using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using System.Net;
using static ModuleManagementBackend.Model.DTOs.HolidayCalenderDTO.HolidayCalenderCommonDTO;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IModuleManagementService
    {
        Task<ResponseModel> EditEmployeeProfileAsync(EditEmployeeDto incoming);
        Task<ResponseModel> GetAllEditEmployeeRequests(string? employeeCode = null, string? location = null, string? userName = null, string? empcode = null, string? autoId = null);
        Task<ResponseModel> ProcessEditEmployeeRequest(AprooveEmployeeReportDto request);
        Task<ResponseModel> EditEmployeeReportingOfficerAsync(EditEmployeeReportDto dto);
        Task<ResponseModel> GetAllReportingOfficerRequest(string? employeeCode = null, string? location = null, string? userName = null);
        Task<ResponseModel> ProcessEditReportingOfficerRequest(AprooveEmployeeReportDto request);
        Task<ResponseModel> GetDfccilDirectory(string? EmpCode = null);
        Task<ResponseModel> UpdateDfccilDirectory(UpdateEmployeeDto updateDto);
        Task<ResponseModel> GetAllEmployeeOfTheMonth();
        Task<ResponseModel> GetCurrentEmployeeOfTheMonth();
        Task<ResponseModel> AddEmployeeOfTheMonth(EmployeeOfTheMonthDto dto);


        #region NOTICE BOARD

        Task<ResponseModel> GetAllNotices();
        Task<ResponseModel> AddNotice(NoticeBoardDto dto);
        Task<ResponseModel> UpdateNotice(int id, NoticeBoardDto dto);
        Task<ResponseModel> DeleteNotice(int id);
        Task<ResponseModel> GetAllArchiveNotices();

        #endregion

        #region DEPENDENT

        Task<ResponseModel> AddDependentsAsync(List<AddDependentDto> dependents, string loginUserEmpCode);
        Task<ResponseModel> ProceedDependentsAsync(AprooveEmployeeReportDto request, string loginUserEmpCode);
        Task<ResponseModel> GetAllDependentsRequestByEmpCodeAsync();
        Task<ResponseModel> GetDependentsByEmpCodeAsync(string empCode);
        Task<ResponseModel> GetDependentsListByEmpCodeAsync(string empCode);
        Task<ResponseModel> UpdateDependentAsync(int DependentId, AddDependentDto dto, string loginUserEmpCode);
        #endregion

        #region Contractual Employee Edit Requests
        Task<ResponseModel> GetAllContractualEmployeeEditRequestsAsync();
        Task<ResponseModel> GetAcceptOrRejectContractualEmployeeEditRequestsAsync(int status);
        Task<ResponseModel> ProcessEditContractualEmployeeRequest(AprooveContractualEmployeeDto request, string LoginUserId);
        #endregion

        #region Notification 
        ResponseModel GetEditEmployeeStatus(string EmployeeCode);
        Task<PagedResponseModel> GetSMSLogDetailsPaginatedAsync(SMSLogRequest request);
        Task<ResponseModel> UpdateSMSAsync(int SmsId);
        #endregion

        #region Todo List
        Task<ResponseModel> CreateToDoListAsync(CreateTodoListDto dto);
        Task<ResponseModel> GetToDoListAsync(string employeeCode = "0");
        Task<ResponseModel> UploadAboutUsAsync(UploadAboutUsDto dto);
        #endregion

        #region Employee Badge
        Task<ResponseModel> GetEmployeeProfile(string empCode);
        Task<ResponseModel> GetAllMastersAsync();
        Task<ResponseModel> GetSelectedEmployeeColumnsAsync(string columnNamesCsv, string? employeeCode = null);
        Task<ResponseModel> GetEmployeeMasterColumnsAsync();

        #endregion

        #region Holiday Calendar

        Task<ResponseModel> GetAllHolidays(int? unitId = null, string? holidayType = null, string? unitName = null);

        Task<ResponseModel> GetHolidaysByDateRange(DateTime? fromDate = null, DateTime? toDate = null, int? unitId = null);

        Task<ResponseModel> CreateHoliday(CreateHolidayCalendarDto createHolidayDto, string loginUserId);

        Task<ResponseModel> UpdateHoliday(UpdateHolidayCalendarDto updateHolidayDto, string loginUserId);

        Task<ResponseModel> DeleteHoliday(int holidayId, string? loginUserId = null);

        Task<ResponseModel> BulkCreateHolidays(List<CreateHolidayCalendarDto> holidays, string loginUserId);

        Task<ResponseModel> UploadHolidaysFromExcel(IFormFile file, int unitId, string unitName, string LoginUserId);



        #endregion

        #region Mobile And Email Change Requests

        Task<ResponseModel> ChangeMobileNumberAsync(string userEmpCode, string newMobileNumber, string otp);
        Task<ResponseModel> GenerateOtpAsync(string userEmpCode, string newMobileNumber);
        Task<ResponseModel> RequestEmailChangeAsync(string userEmpCode, string newEmail);
        Task<ResponseModel> VerifyEmailChangeAsync(Guid token);
        #endregion

        Task<ResponseModel> GetKraReporingOfficer(string empCode, DateTime startDate, DateTime endDate);
    }
}
