using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;

namespace ModuleManagementBackend.API.Controllers
{

    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleManagementController : ControllerBase
    {
        private readonly IModuleManagementService managementService;
        private readonly IHttpContextAccessor httpContext;

        public ModuleManagementController(IModuleManagementService managementService, IHttpContextAccessor httpContext)
        {
            this.managementService=managementService;
            this.httpContext=httpContext;
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
            // httpContext.HttpContext.Items.TryGetValue("UserName", out var userName);
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
    }

}
