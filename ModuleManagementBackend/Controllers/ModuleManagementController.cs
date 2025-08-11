using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using static ModuleManagementBackend.BAL.Services.AccountService;

namespace ModuleManagementBackend.API.Controllers
{

  
    [Route("api/[controller]")]
    [ApiController]

    [AllowAnonymous]
    public class ModuleManagementController : ControllerBase
    {
        private readonly IModuleManagementService managementService;
        private readonly IHttpContextAccessor httpContext;
       

        public ModuleManagementController(IModuleManagementService managementService, IHttpContextAccessor httpContext)
        {
            this.managementService=managementService;
            this.httpContext=httpContext;

            
        }

        private string LoginUserId
        {
            get
            {
                var currentUser = httpContext.HttpContext?.Items["CurrentUser"] as Root;
                var userId = currentUser?.UserId ?? "0";
                return userId;
            }
        }

        [HttpGet("ModuleMangement/GetAllEditEmployeeRequest")]
        public async Task<ResponseModel> GetAllEditEmployeeRequest(string? employeeCode = null, string? location = null, string? userName = null)
        {
            return await managementService.GetAllEditEmployeeRequests(employeeCode, location, userName);
        }

        [HttpPut("ModuleMangement/ProceedEditEmployeeRequest")]
        public async Task<ResponseModel> ProceedEditEmployeeRequest([FromBody] AprooveEmployeeReportDto aprooveEmployee)
        {
            return await managementService.ProcessEditEmployeeRequest(aprooveEmployee);
        }

        [HttpGet("ModuleMangement/GetAllReportingOfficerRequest")]
        public async Task<ResponseModel> GetAllReportingOfficerRequest(string? employeeCode = null, string? location = null, string? userName = null)
        {
            return await managementService.GetAllReportingOfficerRequest(employeeCode, location, userName);
        }

        [HttpPut("ModuleMangement/ProceedReportingOfficerRequest")]
        public async Task<ResponseModel> ProceedReportingOfficerRequest([FromBody] AprooveEmployeeReportDto aprooveEmployee)
        {
            return await managementService.ProcessEditReportingOfficerRequest(aprooveEmployee);
        }

        [HttpGet("ModuleMangement/GetDfccilDirctory")]
        public async Task<ResponseModel> GetDfccilDirctory(string? employeeCode = null)
        {
            return await managementService.GetDfccilDirectory(employeeCode);
        }

        [HttpPut("ModuleMangement/UpdateDfccilDiractory")]
        public async Task<ResponseModel> UpdateDfccilDiractory([FromBody] UpdateEmployeeDto Employee)
        {
            return await managementService.UpdateDfccilDirectory(Employee);
        }

        [HttpGet("GetAllEmployeeOfTheMonth")]
        public async Task<ResponseModel> GetAllEmployeeOfTheMonth()
        {
            var response = await managementService.GetAllEmployeeOfTheMonth();

            return response;
        }

        [HttpGet("GetCurrentEmployeeOfTheMonth")]
        public async Task<ResponseModel> GetCurrentEmployeeOfTheMonth()
        {
            var response = await managementService.GetCurrentEmployeeOfTheMonth();

            return response;
        }

        [HttpPost("AddEmployeeOfTheMonth")]
        public async Task<ResponseModel> AddEmployeeOfTheMonth([FromForm] EmployeeOfTheMonthDto dto)
        {
            var response = await managementService.AddEmployeeOfTheMonth(dto);

            return  response;
        }


        #region NOTICE BOARD

        [HttpGet("GetAllNoticeBoard")]
        public async Task<ResponseModel> GetAllNoticeBoard()
        {
            var response = await managementService.GetAllNotices();
            return response;
        }

        [HttpGet("GetAllArchiveNotices")]
        public async Task<ResponseModel> GetAllArchiveNotices()
        {
            var response = await managementService.GetAllArchiveNotices();
            return  response;
        }



        [HttpPost("CreateNoticeBoard")]
        public async Task<IActionResult> CreateNoticeBoard([FromForm] NoticeBoardDto dto)
        {
            var response = await managementService.AddNotice(dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("UpdateNoticeBoard/{id}")]
        public async Task<IActionResult> UpdateNoticeBoard(int id, [FromForm] NoticeBoardDto dto)
        {
            var response = await managementService.UpdateNotice(id, dto);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("DeleteNoticeBoard/{id}")]
        public async Task<IActionResult> DeleteNoticeBoard(int id)
        {
            var response = await managementService.DeleteNotice(id);
            return StatusCode((int)response.StatusCode, response);
        }

        #endregion

        #region DEPENDENT

        [HttpPost("addDependentList")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddDependents([FromForm] List<AddDependentDto> request)
        {
            var response = await managementService.AddDependentsAsync(request, LoginUserId);
            return StatusCode((int)response.StatusCode, response);
        }


        [HttpPut("proceedDependentRequest")]
        public async Task<ResponseModel> ProceedDependents([FromBody] AprooveEmployeeReportDto request)
        {
            var response = await managementService.ProceedDependentsAsync(request, LoginUserId);
            return response;
        }

        
        [HttpGet("GetAllDependentRequests")]
        public async Task<ResponseModel> GetAllPendingRequests()
        {
            var response = await managementService.GetAllDependentsRequestByEmpCodeAsync();
            return response;
        }

        
        [HttpGet("GetAllDependentRequestsByCreatedBy/{empCode}")]
        public async Task<ResponseModel> GetDependentsByEmpCode(string empCode)
        {
            var response = await managementService.GetDependentsByEmpCodeAsync(empCode);
            return response;
        }

        #endregion

        #region CONTRACTUAL EMPLOYEE

        [HttpGet("GetAllContractualEmployeeRequests")]
        public async Task<ResponseModel> GetAllContractualEmployeeEditRequests()
        {
            return await managementService.GetAllContractualEmployeeEditRequestsAsync();
        }
        [HttpPut("ProcessContractualEmployeeRequest")]

        public async Task<ResponseModel> ProcessEditContractualEmployeeRequest([FromBody] AprooveContractualEmployeeDto request)
        {
            return await managementService.ProcessEditContractualEmployeeRequest(request, LoginUserId);
        }
        #endregion
    }

}
