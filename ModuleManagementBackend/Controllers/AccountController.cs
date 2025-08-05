using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.Model.Common;
using Microsoft.AspNetCore.Mvc;
using static ModuleManagementBackend.BAL.Services.AccountService;

namespace ModuleManagementBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly IAccountService _accountRepository;
        private readonly IConfiguration _configuration;

        public AccountController(IAccountService accountRepository, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
        }


        [HttpPost("IsValidProgress")]
        public async Task<ResponseModel> IsValidProgress(string Token, string EmpCode)
        {
            var response = await _accountRepository.IsValidProgress(Token, EmpCode);
            return response;
        }


        // Add this method to your existing AccountController.cs in the API project
        // Add this after your existing IsValidProgress method

        [HttpGet("profile")]
        public async Task<ResponseModel> GetUserProfile()
        {
            var responseModel = new ResponseModel();

            try
            {
                // Get user info from the filter (stored in HttpContext.Items)
                var currentUser = HttpContext.Items["CurrentUser"] as Root;

                if (currentUser == null)
                {
                    responseModel.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    responseModel.Message = "User not found in context";
                    return responseModel;
                }

                var empCode = Convert.ToInt32(currentUser.username);

                // Get employee details from your database (reusing your existing logic)
                var employeeDetails = await _accountRepository.GetEmployeeDetailsAsync(empCode);

                if (employeeDetails != null)
                {
                    // Get roles from your database using your existing logic
                    var isSuperAdmin = _configuration["SuperAdmin"]?.ToString() == employeeDetails.empCode;
                    List<string> userRoles = new List<string>();

                    if (!isSuperAdmin)
                    {
                        // Use your existing role fetching logic
                        var roles = await _accountRepository.GetUserRolesAsync(empCode);
                        userRoles = roles.Any() ? roles : new List<string> { "user" };
                    }
                    else
                    {
                        userRoles.Add("superAdmin");
                    }

                    var userProfile = new
                    {
                        EmpId = employeeDetails.empId,
                        EmpCode = employeeDetails.empCode,
                        Name = employeeDetails.empName,
                        Email = employeeDetails.empEmail,
                        Mobile = employeeDetails.empMobileNo,
                        Designation = employeeDetails.designation,
                        Unit = employeeDetails.units,
                        UnitId = employeeDetails.unitId,
                        Department = employeeDetails.department,
                        Level = employeeDetails.lavel,
                        Roles = userRoles,
                        // Include SSO user info
                        SSOUserInfo = new
                        {
                            currentUser.username,
                            // currentUser.username,
                            currentUser.UnitName,
                            currentUser.UnitId,
                            currentUser.Designation,
                            currentUser.Level,
                            currentUser.Department
                        }
                    };

                    responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                    responseModel.Message = "Profile retrieved successfully";
                    responseModel.Data = userProfile;
                }
                else
                {
                    responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                    responseModel.Message = "Employee details not found";
                }
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                responseModel.Message = "An error occurred while retrieving profile";

            }

            return responseModel;
        }

        [HttpGet("user-roles/{empCode}")]
        public async Task<ResponseModel> GetUserRoles(int empCode)
        {
            var responseModel = new ResponseModel();

            try
            {
                var roles = await _accountRepository.GetUserRolesAsync(empCode);

                responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                responseModel.Message = "Roles retrieved successfully";
                responseModel.Data = new { Roles = roles };
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                responseModel.Message = "An error occurred while retrieving roles";
            }

            return responseModel;
        }

    }
}
