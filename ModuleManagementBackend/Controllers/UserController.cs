using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.UserDTO;
using System.Net;
namespace ModuleManagementBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {

        private readonly IUserService _userrepository;
        public UserController(IHttpContextAccessor httpContext, IUserService userrepository) : base(httpContext)
        {
            _userrepository = userrepository;
        }
        [HttpPost("AddNewRoleWithPermission")]
        public async Task<ResponseModel> AddNewRoleWithPermission(DtoAddUserRole model)
        {

            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();

            var source = SourceType;

            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }


            if (string.IsNullOrWhiteSpace(source))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Source is missing in Header.";
                return response;
            }
            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }


            response = await _userrepository.AddNewRoleWithPermission(model, empCode, source);

            return response;

        }

        [HttpPut("EditEmpRoleWithPermission")]
        public async Task<ResponseModel> EditRoleWithPermission(DtoEditUserRolesByEmpCode model)
        {

            var empCode = TokenDetails.EmpCode;

            var response = new ResponseModel();

            var source = SourceType;

            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }



            if (string.IsNullOrWhiteSpace(source))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Source is missing in Header.";
                return response;
            }
            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }


            response = await _userrepository.EditUserRolesAndPermissionsByEmpCode(model, empCode, source);

            return response;

        }

        [HttpPut("EditEmpRole")]
        public async Task<ResponseModel> EditEmpRole(DtoEditEmpRole model)
        {

            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();
            var unit = TokenDetails.Unit;
            var source = SourceType;

            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }


            //if (string.IsNullOrWhiteSpace(source))
            //{
            //    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            //    response.Message = "Source is missing in Header.";
            //    return response;
            //}
            if (string.IsNullOrWhiteSpace(unit))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Unit is missing in Token.";
                return response;
            }
            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }
            response = await _userrepository.EditEmpRole(model, empCode, source);

            return response;

        }

        [HttpPut("EditRolePermissions")]
        public async Task<ResponseModel> EditRolePermissions(DtoEditRolePermissions model)
        {

            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();

            var source = SourceType;
            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }


            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }
            response = await _userrepository.EditRolePermissions(model, empCode, source);

            return response;

        }

        [HttpPost("AddNewPermission")]
        public async Task<ResponseModel> AddNewPermission(DtoAddUserPermission model)
        {
            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();
            var source = SourceType;
            source = "swagger";


            if (string.IsNullOrWhiteSpace(source))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Source is missing in Header.";
                return response;
            }
            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }


            response = await _userrepository.AddNewPermission(model, empCode, source);

            return response;

        }

        [HttpGet("GetRoleAndPermissionByEmpCode/{empCode}")]
        public async Task<ResponseModel> GetRoleAndPermissionByEmpCode(int empCode)
        {

            var response = new ResponseModel();
            var source = SourceType;

            response = await _userrepository.GetRoleAndPermissionByEmpCode(empCode);

            return response;
        }

        [HttpGet("GetEmpRoleList")]
        public async Task<ResponseModel> GetEmpRoleList()
        {

            var response = new ResponseModel();
            var source = SourceType;
            var unit = TokenDetails.Unit;
            var empCode = TokenDetails.EmpCode;
          
            if (string.IsNullOrWhiteSpace(unit))
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Unit is missing in Token.";
                return response;
            }
            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }
            response = await _userrepository.GetEmpRoleList(Convert.ToInt32(TokenDetails.UnitID), empCode);

            return response;
        }

        [HttpGet("GetRoleAndPermissionList")]
        public async Task<ResponseModel> GetRoleAndPermissionList()
        {

            var response = new ResponseModel();
            response = await _userrepository.GetRoleAndPermissionList();

            return response;
        }


        [HttpPost("AddUserRoleMapping")]
        public async Task<ResponseModel> AddUserRoleMapping(AddUserRoleMapping model)
        {

            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();
            var source = SourceType;

            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }


            response = await _userrepository.AddUserRoleMapping(model, empCode, source);

            return response;
        }

        [HttpGet("GetPermissionList")]
        public async Task<ResponseModel> GetPermissionList()
        {
            var response = new ResponseModel();
            var source = SourceType;
            
            response = await _userrepository.GetPermissionList();
            return response;
        }

        [HttpPost("AddNewRole")]
        public async Task<ResponseModel> AddNewRole(DtoAddUserRoleWithoutPermission model)
        {

            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();


            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }


            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }


            response = await _userrepository.AddNewRole(model, empCode);

            return response;

        }

        [HttpPut("UpdateRole")]
        public async Task<ResponseModel> UpdateRole(int roleId, DtoAddUserRoleWithoutPermission model, int EmpCode)
        {
            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();
            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }
            if (!ModelState.IsValid)
            {

                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Model is not valid.";
                response.Data = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return response;
            }
            response = await _userrepository.UpdateRole(roleId,model, empCode);
            return response;

        }

        [HttpDelete("DeleteRole")]
        public async Task<ResponseModel> DeleteRole(int roleId, int EmpCode)
        {
            var empCode = TokenDetails.EmpCode;
            var response = new ResponseModel();
            if (empCode <= 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.Message = "Emp code is missing or invalid in token.";
                return response;
            }
            response = await _userrepository.DeleteRole(roleId, EmpCode);
            return response;

        }

        [HttpGet("GetAllRoles")]
        public async Task<ResponseModel> GetAllRoles()
        {
            var response = new ResponseModel();
            response = await _userrepository.GetAllRoles();
            return response;

        }

        [HttpGet("GetRoleById")]
        public async Task<ResponseModel> GetRoleById(int roleId)
        {
            var response = new ResponseModel();
            response = await _userrepository.GetRoleById(roleId);
            return response;

        }
    }

}


