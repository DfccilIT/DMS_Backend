using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;

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
        #endregion
    }
}
