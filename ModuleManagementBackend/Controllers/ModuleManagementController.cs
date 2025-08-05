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

        public ModuleManagementController(IModuleManagementService managementService)
        {
            this.managementService=managementService;
        }

        [HttpPost("ModuleMangement/ProceedEditEmployeeRequest")]
        public async Task<ResponseModel> ProceedEditEmployeeRequest([FromBody] AprooveEmployeeReportDto aprooveEmployee)
        {
            return await managementService.ProcessEditEmployeeRequest(aprooveEmployee);
        }

        [HttpGet("ModuleMangement/GetAllReportingOfficerRequest")]
        public async Task<ResponseModel> GetAllReportingOfficerRequest(string? employeeCode = null, string? location = null, string? userName = null)
        {
            return await managementService.GetAllReportingOfficerRequest(employeeCode, location, userName);
        }

        [HttpPost("ModuleMangement/ProceedReportingOfficerRequest")]
        public async Task<ResponseModel> ProceedReportingOfficerRequest([FromBody] AprooveEmployeeReportDto aprooveEmployee)
        {
            return await managementService.ProcessEditReportingOfficerRequest(aprooveEmployee);
        }

        [HttpGet("ModuleMangement/GetDfccilDirctory")]
        public async Task<ResponseModel> GetDfccilDirctory(string? employeeCode = null)
        {
            return await managementService.GetDfccilDirectory(employeeCode);
        }

    }
}
