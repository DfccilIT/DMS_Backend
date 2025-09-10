using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.BAL.Services;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
using static ModuleManagementBackend.BAL.Services.AccountService;

namespace ModuleManagementBackend.API.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    
    public class ModuleManagementController : ControllerBase
    {
        private readonly IModuleManagementService managementService;
        private readonly IHttpContextAccessor httpContext;
        private readonly IDapperService dapper;
        private readonly IConfiguration configuration;

        public ModuleManagementController(IModuleManagementService managementService, IHttpContextAccessor httpContext, IDapperService dapper, IConfiguration configuration)
        {
            this.managementService=managementService;
            this.httpContext=httpContext;
            this.dapper=dapper;
            this.configuration=configuration;
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

        [HttpPost("EditEmployee")]
        public async Task<ResponseModel> EditEmployeeProfile(EditEmployeeDto dto)
        {
            return await managementService.EditEmployeeProfileAsync(dto);
        }


        [HttpPost("UpdatePersonalEmail")]
        public async Task<ResponseModel> UpdatePersonalEmail([FromBody] RequestEmailDto dto)
        {
            return await managementService.UpdatePersonalEmailAsync(dto.UserEmpCode, dto.NewEmail, LoginUserId);
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

        [HttpPost("EditEmployeeReportingOfficer")]
        public async Task<ResponseModel> EditEmployeeReportingOfficer(EditEmployeeReportDto dto)
        {
            return await managementService.EditEmployeeReportingOfficerAsync(dto);
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

            return response;
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
            return response;
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


        [HttpGet("GetAllDependentsByEmpCode/{empCode}")]
        public async Task<ResponseModel> GetAllDependents(string empCode)
        {
            var response = await managementService.GetDependentsListByEmpCodeAsync(empCode);
            return response;
        }

        [HttpPut("UpdateDependent/{DependentId}")]
        public async Task<ResponseModel> UpdateDependent(int DependentId, [FromForm] AddDependentDto dto)
        {
            return await managementService.UpdateDependentAsync(DependentId, dto, LoginUserId);

        }

        #endregion

        #region CONTRACTUAL EMPLOYEE

        [HttpGet("GetAllContractualEmployeeRequests")]
        public async Task<ResponseModel> GetAllContractualEmployeeEditRequests()
        {
            return await managementService.GetAllContractualEmployeeEditRequestsAsync();
        }
        [HttpGet("GetAcceptOrRejectContractualEmployeeRequests/{Status}")]
        public async Task<ResponseModel> GetAcceptOrRejectContractualEmployeeRequests(int Status)
        {
            return await managementService.GetAcceptOrRejectContractualEmployeeEditRequestsAsync(Status);
        }
        [HttpPut("ProcessContractualEmployeeRequest")]

        public async Task<ResponseModel> ProcessEditContractualEmployeeRequest([FromBody] AprooveContractualEmployeeDto request)
        {
            return await managementService.ProcessEditContractualEmployeeRequest(request, LoginUserId);
        }
        #endregion

        [HttpGet("GetEmployeeProfile/{empCode}")]
        public async Task<ResponseModel> GetEmployeeProfile(string empCode)
        {
            return await managementService.GetEmployeeProfile(empCode);
        }

        [HttpGet]
        [Route("GetEditEmployeeStatus/{EmployeeCode}")]
        public ResponseModel GetEditEmployeeStatus(string EmployeeCode)
        {
            var response = managementService.GetEditEmployeeStatus(EmployeeCode);
            if (response == null)
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Employee not found"
                };
            }
            return response;
        }


        [HttpGet("GetNotificationList")]
        public async Task<IActionResult> GetSMSLogDetailsPaginatedGet([FromQuery] SMSLogRequest request)
        {
            var result = await managementService.GetSMSLogDetailsPaginatedAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("UpdateNotification")]
        public async Task<IActionResult> UpdateNotification(int SmsId)
        {
            var result = await managementService.UpdateSMSAsync(SmsId);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("CreateToDoList")]
        public async Task<ResponseModel> CreateToDoList(CreateTodoListDto dto)
        {
            return await managementService.CreateToDoListAsync(dto);
        }
        [HttpGet("GetToDoList/{employeeCode}")]
        public async Task<ResponseModel> GetToDoList(string employeeCode)
        {
            return await managementService.GetToDoListAsync(employeeCode);
        }

        [HttpPost("UploadAboutUs")]
        public async Task<IActionResult> UploadAboutUs([FromForm] UploadAboutUsDto dto)
        {
            var result = await managementService.UploadAboutUsAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("GetAllMasterData")]
        public async Task<ResponseModel> GetAllMasterData()
        {
            return await managementService.GetAllMastersAsync();
        }

        [HttpGet("GetEmployeeColumn")]

        public async Task<ResponseModel> GetEmployeeColumn()
        {
            return await managementService.GetEmployeeMasterColumnsAsync();
        }

        [HttpGet("GetEmployeeList")]

        public async Task<ResponseModel> GetEmployeeList(string columnNamesCsv, string? empCode = null)
        {
            return await managementService.GetSelectedEmployeeColumnsAsync(columnNamesCsv, empCode);
        }

        [AllowAnonymous]
        [HttpPost("generate-otp")]
        public async Task<ResponseModel> GenerateOtp([FromBody] GenerateOtpDto dto)
        {
            var result = await managementService.GenerateOtpAsync(dto.UserEmpCode, dto.NewMobileNumber);
            return result;
        }


        [HttpPost("change-mobile-number")]
        public async Task<ResponseModel> ChangeMobileNumber([FromBody] ChangeMobileNumberDto dto)
        {
            var result = await managementService.ChangeMobileNumberAsync(dto.UserEmpCode, dto.NewMobileNumber, dto.Otp);
            return result;
        }


        [HttpPost("request-email-change")]
        public async Task<ResponseModel> RequestEmailChange([FromBody] RequestEmailDto dto)
        {
            var result = await managementService.RequestEmailChangeAsync(dto.UserEmpCode, dto.NewEmail);
            return result;
        }


        [HttpGet("verify-email-change")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailChange([FromQuery] Guid token)
        {
            var result = await managementService.VerifyEmailChangeAsync(token);
            var environment = configuration["DeploymentModes"]??string.Empty;


            string baseUrl = $"{Request.Scheme}://{Request.Host}/email-change/result.html";
            string redirectUrl;

            if (result.StatusCode == HttpStatusCode.OK)
            {

                redirectUrl = $"{baseUrl}?status=success&email={Uri.EscapeDataString(result.Data?.ToString() ?? "")}&Env={environment}";
            }
            else
            {

                redirectUrl = $"{baseUrl}?status=error&msg={Uri.EscapeDataString(result.Message ?? "Verification failed")}&Env={environment}";
            }

            return Redirect(redirectUrl);
        }

        [HttpPut("UpdateExtensionNo")]
        public async Task<IActionResult> UpdateExtensionNo(
        [FromQuery] string employeeCode,
        [FromQuery] string extensionNo
       )
        {
            if (string.IsNullOrWhiteSpace(employeeCode) || string.IsNullOrWhiteSpace(extensionNo))
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Employee code and extension number are required"
                });
            }

            var response = await managementService.UpdateExtensionNoAsync(employeeCode, extensionNo, LoginUserId);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response);

            return StatusCode((int)response.StatusCode, response);
        }


        [HttpGet("GetReportingOfficers")]

        public async Task<ResponseModel> GetReportingOfficers(string empCode, DateTime startDate, DateTime endDate)
        {
            var result = await managementService.GetKraReporingOfficer(empCode, startDate, endDate);

            return result;
        }

        [HttpPost("uploadEmployeeProfilePhoto")]
        public async Task<IActionResult> UploadEmployeeThreeWayPhotos(
       string employeeCode, IFormFile leftImage, IFormFile centerImage, IFormFile rightImage)
        {
            var result = await managementService.UploadEmployeeThreeWayPhotos(employeeCode, leftImage, centerImage, rightImage);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("GetEmployeeProfilePhoto/{employeeCode}")]
        public async Task<IActionResult> GetEmployeeThreeWayPhotos(string employeeCode)
        {
            var result = await managementService.GetEmployeeThreeWayPhotos(employeeCode);
            return StatusCode((int)result.StatusCode, result);
        }
    }

}


