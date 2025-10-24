using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using System;
using System.Data;
using static ModuleManagementBackend.BAL.Services.AccountService;

namespace ModuleManagementBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public readonly IAccountService _accountRepository;
        private readonly IConfiguration _configuration;
        private readonly SAPTOKENContext context;

        public AccountController(IAccountService accountRepository, IConfiguration configuration, SAPTOKENContext _context)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
            context=_context;
        }


       

        [HttpGet("profile")]
        public async Task<ResponseModel> GetUserProfile()
        {
            var responseModel = new ResponseModel();

            try
            {

                var currentUser = HttpContext.Items["CurrentUser"] as Root;

                if (currentUser == null)
                {
                    responseModel.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    responseModel.Message = "User not found in context";
                    return responseModel;
                }

                var empCode = Convert.ToInt32(currentUser.username);


                var employeeDetails = await context.MstEmployeeMasters
                    .Join(context.UnitNameDetails,
                          Emp => Emp.Location,
                          Unit => Unit.Name,
                          (Emp, Unit) => new
                          {
                              Emp,
                              Unit.Id
                          }

                    )
                    //.Join(context.UnitNameDetails,
                    //      Emp => Emp.Location,
                    //      Unit => Unit.Name,
                    //      (Emp, Unit) => new
                    //      {
                    //          Emp,
                    //          Unit.Id
                    //      }

                    //)

                    .FirstOrDefaultAsync(x => x.Emp.EmployeeCode==currentUser.username);

                if (employeeDetails != null)
                {

                    var isSuperAdmin = _configuration["SuperAdmin"]?.ToString() == employeeDetails.Emp.EmployeeCode;
                    List<string> userRoles = new List<string>();

                    if (!isSuperAdmin)
                    {

                        var roles = await _accountRepository.GetUserRolesAsync(empCode);
                        userRoles = roles.Any() ? roles : new List<string> { "user" };
                    }
                    else
                    {
                        userRoles.Add("superAdmin");
                    }
                    var cadreData = await GetCadreDataAsync();


                    var unitAssigned = (
                           from cadre in cadreData
                           where cadre.EmployeeCode == employeeDetails.Emp.EmployeeCode
                                 && cadre.RoleAssigned.Equals("CGM")
                           select new
                           {
                               RoleAssign = cadre.RoleAssigned,
                               UnitAssign = new
                               {
                                   UnitId = cadre.AssignedUnitId,
                                   UnitName = cadre.AssignedUnit,
                               }

                           }
                       )
                       .Distinct()
                       .ToList();


                    if (employeeDetails.Emp.Post.Equals("CGM", StringComparison.OrdinalIgnoreCase))
                    {
                        unitAssigned.Add(new
                        {
                            RoleAssign = "CGM",
                            UnitAssign = new
                            {
                                UnitId = employeeDetails.Id,
                                UnitName = employeeDetails.Emp.Location,
                            }
                        });
                    }
                    var MainunitAssigned = unitAssigned.GroupBy(x => x.RoleAssign)
                        .Select(g => new
                        {
                            RoleAssign = g.Key,
                            Units = g.Select(u => new
                            {
                                UnitId = u.UnitAssign.UnitId,
                                UnitName = u.UnitAssign.UnitName
                            })
                             .Distinct()
                             .ToList()
                        })
                        .ToList();
                    var userProfile = new
                    {
                        EmpId = employeeDetails.Emp.EmployeeMasterAutoId,
                        EmpCode = employeeDetails.Emp.EmployeeCode,
                        Name = employeeDetails.Emp.UserName,
                        Email = employeeDetails.Emp.emailAddress,
                        Mobile = employeeDetails.Emp.Mobile,
                        Designation = employeeDetails.Emp.Post,
                        Unit = employeeDetails.Emp.Location,
                        UnitId = employeeDetails.Id,
                        Department = employeeDetails.Emp.DeptDFCCIL,
                        Level = employeeDetails.Emp.PositionGrade,
                        DMSRoles = userRoles,
                        GlobelAssigndRolesAndUnits = MainunitAssigned
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

        private async Task<List<AssignedUnitView>> GetCadreDataAsync()
        {
            var cadreData = new List<AssignedUnitView>();

            using (var connection = context.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [dbo].[vCadreAllotement]";
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new AssignedUnitView
                            {
                                AutoId = reader.GetInt64(reader.GetOrdinal("AutoId")),
                                EmployeeCode = reader["EmployeeCode"]?.ToString(),
                                Unit = reader["Unit"]?.ToString(),
                                UnitId = reader.GetInt32(reader.GetOrdinal("UnitId")),
                                RoleAssigned = reader["RoleAssigned"]?.ToString(),
                                AssignedUnit = reader["AssignedUnit"]?.ToString(),
                                AssignedUnitId = reader.GetInt32(reader.GetOrdinal("AssignedUnitId")),
                                AssignedDepartment = reader["AssignedDepartment"]?.ToString(),
                                AssignedGrades = reader["AssignedGrades"]?.ToString()
                            };

                            cadreData.Add(item);
                        }
                    }
                }
            }

            return cadreData;
        }
        public class AssignedUnitView
        {
            public long AutoId { get; set; }

            public string EmployeeCode { get; set; }

            public string Unit { get; set; }

            public int UnitId { get; set; }

            public string RoleAssigned { get; set; }

            public string AssignedUnit { get; set; }

            public int AssignedUnitId { get; set; }

            public string AssignedDepartment { get; set; }

            public string AssignedGrades { get; set; }
        }

    }
}
