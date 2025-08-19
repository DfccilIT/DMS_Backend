using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;
using static ModuleManagementBackend.BAL.Services.AccountService;
using static ModuleManagementBackend.Model.DTOs.PoliciesGenricDTO.PoliciesCommonDTO;

namespace ModuleManagementBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class PoliciesManagementController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        private readonly IHttpContextAccessor httpContext;

        public PoliciesManagementController(IPolicyService policyService, IHttpContextAccessor httpContext)
        {

            this._policyService=policyService;
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
        [HttpGet("GetAllPolicies")]
        [AllowAnonymous]
        public async Task<ResponseModel> GetAllPolicies()
        {
            var response = await _policyService.GetAllPolicies();
            return response;
        }

        

       
        [HttpPost("AddPolicy")]
        public async Task<ResponseModel> AddPolicy([FromForm] AddPolicyDto dto)
        {
            var response = await _policyService.AddPolicy(dto,LoginUserId);
            return response;
        }

       
        [HttpPut("UpdatePolicy/{id}")]
        public async Task<ResponseModel> UpdatePolicy(int id, [FromForm] UpdatePolicyDto dto)
        {
            var response = await _policyService.UpdatePolicy(id, dto,LoginUserId);
            return response;
        }

        
        [HttpDelete("DeletePolicy/{id}")]
        public async Task<ResponseModel> DeletePolicy(int id)
        {
            var response = await _policyService.DeletePolicy(id);
            return response;
        }

       
        [HttpPost("AddPolicyItem")]
        public async Task<ResponseModel> AddPolicyItem([FromForm] AddPolicyItemDto dto)
        {
            var response = await _policyService.AddPolicyItem(dto, LoginUserId);
            return response;
        }

        
        [HttpPut("UpdatePolicyItem/{id}")]
        public async Task<ResponseModel> UpdatePolicyItem(int id, [FromForm] UpdatePolicyItemDto dto)
        {
            var response = await _policyService.UpdatePolicyItem(id, dto, LoginUserId);
            return response;
        }

        
        [HttpDelete("DeletePolicyItem/{id}")]
        public async Task<ResponseModel> DeletePolicyItem(int id)
        {
            var response = await _policyService.DeletePolicyItem(id);
            return response;
        }
    }

}

