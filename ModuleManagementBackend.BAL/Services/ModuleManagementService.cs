using ClosedXML.Excel;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.BAL.IServices.ICacheServices;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using ModuleManagementBackend.Model.DTOs.GETEMPLOYEEDTO;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using static ModuleManagementBackend.Model.DTOs.HolidayCalenderDTO.HolidayCalenderCommonDTO;

namespace ModuleManagementBackend.BAL.Services
{
    public class ModuleManagementService : IModuleManagementService
    {
        private readonly SAPTOKENContext context;
        private readonly IDapperService dapper;
        private readonly IHttpContextAccessor httpContext;
        private readonly IConfiguration configuration;
        private readonly ICacheService _cacheService;
        private readonly IDatabaseChangesService _dbChangeService;
        private readonly ILogger<ModuleManagementService> _logger;
        private readonly string baseUrl;

        public ModuleManagementService(SAPTOKENContext _context, IDapperService _dapper, IHttpContextAccessor _httpContext, IConfiguration _configuration, ICacheService CacheService, IDatabaseChangesService DbChangeService, ILogger<ModuleManagementService> logger)
        {
            context=_context;
            dapper=_dapper;
            httpContext=_httpContext;
            configuration=_configuration;
            baseUrl = ((configuration["DeploymentModes"] ?? string.Empty) == "DFCCIL_UAT")
    ? (configuration["BasePathUat"] ?? string.Empty)
    : (configuration["BasePathProd"] ?? string.Empty);
            _cacheService = CacheService;
            _dbChangeService = DbChangeService;
            _logger = logger;
        }

        public async Task<ResponseModel> EditEmployeeProfileAsync(EditEmployeeDto dto)
        {
            var response = new ResponseModel();

            if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Employee code is required.";
                return response;
            }

            var master = await context.MstEmployeeMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.EmployeeCode.Equals(dto.EmployeeCode, StringComparison.OrdinalIgnoreCase));

            if (master == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Employee not found.";
                return response;
            }

            bool existsInEditTable = await context.EditEmployeeDetails
                .AnyAsync(x => x.EmployeeCode == dto.EmployeeCode && x.status == 99 && x.TableName == "P");

            if (existsInEditTable)
            {
                response.StatusCode = HttpStatusCode.Conflict;
                response.Message = "Request already exists.";
                return response;
            }


            bool isDifferent =
                (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != master.UserName) ||
                (!string.IsNullOrWhiteSpace(dto.Gender) && dto.Gender != master.Gender) ||
                (!string.IsNullOrWhiteSpace(dto.Designation) && dto.Designation != master.Post) ||
                (!string.IsNullOrWhiteSpace(dto.PositionGrade) && dto.PositionGrade != master.PositionGrade) ||
                (!string.IsNullOrWhiteSpace(dto.Department) && dto.Department != master.DeptDFCCIL) ||
                (dto.DOB.HasValue && dto.DOB != master.DOB) ||
                (dto.DateOfAnniversary.HasValue && dto.DateOfAnniversary != master.AnniversaryDate) ||
                (dto.DateOfJoining.HasValue && dto.DateOfJoining != master.DOJDFCCIL) ||
                (!string.IsNullOrWhiteSpace(dto.Location) && dto.Location != master.Location) ||
                (!string.IsNullOrWhiteSpace(dto.Mobile) && dto.Mobile != master.Mobile) ||
                (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != master.emailAddress) ||
                (!string.IsNullOrWhiteSpace(dto.PersonalEmailId) && dto.PersonalEmailId != master.PersonalEmailAddress) ||
                (!string.IsNullOrWhiteSpace(dto.Toemploy) && dto.Toemploy != master.TOemploy) ||
                (!string.IsNullOrWhiteSpace(dto.ExtensionNo) && dto.ExtensionNo != master.ExtnNo);

            if (!isDifferent)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Message = "No changes found.";
                return response;
            }


            var entity = new EditEmployeeDetail
            {
                EmployeeCode = dto.EmployeeCode,
                UserName = dto.UserName,
                Gender = dto.Gender,
                Designation = dto.Designation,
                PositionGrade = dto.PositionGrade,
                Department = dto.Department,
                DOB = dto.DOB,
                DateOfAnniversary = dto.DateOfAnniversary,
                DateOfJoining = dto.DateOfJoining,
                Location = dto.Location,
                Mobile = dto.Mobile,
                Email = dto.Email,
                PersonalEmailId = dto.PersonalEmailId,
                Toemploy = dto.Toemploy,
                ExtensionNo = dto.ExtensionNo,
                status = 99,
                remarks = "Pending update",
                TableName = "P"
            };

            await context.EditEmployeeDetails.AddAsync(entity);
            await context.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.Created;
            response.Message = "Edit Employee Profile Request Sent successfully.";
            response.Data = entity;
            return response;
        }
        public async Task<ResponseModel> GetAllEditEmployeeRequests(string? employeeCode = null, string? location = null, string? userName = null, string? empcode = null, string? autoId = null)
        {


            ResponseModel responseModel = new ResponseModel();




            var query = context.EditEmployeeDetails
             .Join(context.MstEmployeeMasters,
                   ee => ee.EmployeeCode,
                   mm => mm.EmployeeCode,
                   (ee, mm) => new { ee, mm })


                .Where(e => e.ee.status == 99 && e.ee.TableName=="P");


            if (!string.IsNullOrWhiteSpace(employeeCode))
            {
                query = query.Where(e => e.ee.EmployeeCode == employeeCode);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.ee.Location != null && e.ee.Location.Contains(location));
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(e => e.ee.UserName != null && e.ee.UserName.Contains(userName));
            }

            var result = await query.Select(x => new
            {
                RequestId = x.ee.EdtEmpDetID,
                OldRecored = new
                {
                    x.mm.EmployeeCode,
                    x.mm.UserName,
                    x.mm.Gender,
                    Designation = x.mm.Post,
                    x.mm.PositionGrade,
                    Department = x.mm.DeptDFCCIL,
                    //SubDepartment = x.mm.SubDeptDF,
                    x.mm.DOB,
                    DateOfAnniversary = x.mm.AnniversaryDate,
                    DateOfJoining = x.mm.DOJDFCCIL,
                    x.mm.Location,
                    //SubArea = x.mm.PersonnelSubArea,
                    x.mm.Mobile,
                    x.mm.emailAddress,
                    x.mm.PersonalEmailAddress,
                    TOemploy = x.mm.TOemploy,
                    x.mm.AboutUs,
                    x.mm.Photo,
                    x.mm.ReportingOfficer,
                    ExtensionNo = x.mm.ExtnNo,

                },
                NewRecords = new
                {
                    x.ee.EmployeeCode,
                    x.ee.UserName,
                    x.ee.Gender,
                    x.ee.Designation,
                    x.ee.PositionGrade,
                    x.ee.Department,
                    //x.ee.SubDepartment,
                    x.ee.DOB,
                    x.ee.DateOfAnniversary,
                    x.ee.DateOfJoining,
                    x.ee.Location,
                    //x.ee.SubArea,
                    x.ee.Mobile,
                    emailAddress = x.ee.Email,
                    PersonalEmailAddress = x.ee.PersonalEmailId,
                    TOemploy = x.ee.Toemploy,
                    x.ee.AboutUs,
                    x.ee.Photo,
                    x.ee.ReportingOfficer,
                    x.ee.ExtensionNo,
                },
            }).OrderByDescending(x => x.RequestId).ToListAsync();


            responseModel.Message="Data fetched successfully";
            responseModel.StatusCode=System.Net.HttpStatusCode.OK;
            responseModel.Data = result;
            responseModel.TotalRecords= result.Count;
            return responseModel;

        }
        public async Task<ResponseModel> ProcessEditEmployeeRequest(AprooveEmployeeReportDto request)
        {
            var response = new ResponseModel();

            try
            {
                string employeeCode = request.EmployeeCode;
                bool isApproved = request.IsApproved;
                string remarks = request.Remarks;

                if (string.IsNullOrWhiteSpace(employeeCode))
                {
                    response.Message = "EmployeeCode is required.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }



                if (isApproved == false && string.IsNullOrWhiteSpace(remarks))
                {
                    response.Message = "Remarks are mandatory when rejecting a request.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var editRecord = context.EditEmployeeDetails
                    .FirstOrDefault(e => e.EmployeeCode == employeeCode && e.status == 99 && e.TableName=="P");

                if (editRecord == null)
                {
                    response.Message = "No pending edit request found for this employee.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                if (isApproved)
                {
                    var mstRecord = await context.MstEmployeeMasters
                        .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && e.Status == 0);

                    if (mstRecord == null)
                    {
                        response.Message = $"No active master record found for EmployeeCode {employeeCode}.";
                        response.StatusCode = HttpStatusCode.NotFound;
                        return response;
                    }


                    mstRecord.EmployeeCode = editRecord.EmployeeCode;
                    mstRecord.UserName = editRecord.UserName;
                    mstRecord.Gender = editRecord.Gender;
                    mstRecord.designation = editRecord.Designation;
                    mstRecord.Post = editRecord.Designation;
                    mstRecord.PositionGrade = editRecord.PositionGrade;
                    mstRecord.DeptDFCCIL = editRecord.Department;
                    mstRecord.DOB = editRecord.DOB;
                    mstRecord.AnniversaryDate = editRecord.DateOfAnniversary;
                    mstRecord.DOJDFCCIL = editRecord.DateOfJoining;
                    mstRecord.Location = editRecord.Location;
                    mstRecord.Mobile = editRecord.Mobile;
                    mstRecord.emailAddress = editRecord.Email;
                    mstRecord.PersonalEmailAddress = editRecord.PersonalEmailId;
                    mstRecord.TOemploy = editRecord.Toemploy;
                    mstRecord.ExtnNo = editRecord.ExtensionNo;


                    editRecord.status = 0;
                    mstRecord.Status = 0;

                    context.SaveChanges();


                    response.Message = "Approved successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = new { success = true, ApprovedSuccessfully = true };
                    return response;
                }
                else
                {

                    editRecord.remarks = remarks;
                    editRecord.status = 0;
                    context.SaveChanges();

                    response.Message = "Rejected successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = new { success = true, RejectedSuccessfully = true };
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }
        public async Task<ResponseModel> EditEmployeeReportingOfficerAsync(EditEmployeeReportDto dto)
        {
            var response = new ResponseModel();

            if (dto == null || string.IsNullOrWhiteSpace(dto.EmployeeCode))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Invalid request: Employee code is required.";
                return response;
            }

            var master = await context.MstEmployeeMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.EmployeeCode == dto.EmployeeCode && m.Status == 0);

            if (master == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = $"No employee found with the code '{dto.EmployeeCode}'.";
                return response;
            }

            bool exists = await context.EditEmployeeDetails
                .AnyAsync(x => x.EmployeeCode == dto.EmployeeCode && x.status == 99 && x.TableName == "R");

            if (exists)
            {
                response.StatusCode = HttpStatusCode.Conflict;
                response.Message = "A Reporting officer change request is already pending approval.";
                return response;
            }

            var entity = new EditEmployeeDetail
            {
                TableName = "R",
                EmployeeCode = master.EmployeeCode,
                UserName = master.UserName,
                Gender = master.Gender,
                Designation = master.designation,
                PositionGrade = master.PositionGrade,
                Department = master.DeptDFCCIL,
                SubDepartment = master.SubDeptDF,
                DOB = master.DOB,
                DateOfAnniversary = master.AnniversaryDate,
                DateOfJoining = master.DOJDFCCIL,
                Location = master.Location,
                SubArea = master.PersonnelSubArea,
                Mobile = master.Mobile,
                Email = master.emailAddress,
                PersonalEmailId = master.PersonalEmailAddress,
                Toemploy = master.TOemploy,
                AboutUs = master.AboutUs,
                Photo = master.Photo,
                status = 99,
                ReportingOfficer = dto.ReportingOfficerEmployeeCode,
                remarks = "Request by user"
            };

            await context.EditEmployeeDetails.AddAsync(entity);
            await context.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.Created;
            response.Message = "Your request to update the reporting officer has been submitted successfully.";
            response.Data = entity;

            return response;
        }

        public async Task<ResponseModel> GetAllReportingOfficerRequest(string? employeeCode = null, string? location = null, string? userName = null)
        {

            ResponseModel responseModel = new ResponseModel();
            try
            {


                var query = context.EditEmployeeDetails
            .Join(context.MstEmployeeMasters,
                  ee => ee.EmployeeCode,
                  mm => mm.EmployeeCode,
                  (ee, mm) => new { ee, mm })


               .Where(e => e.ee.status == 99 && e.ee.TableName=="R");
                if (!string.IsNullOrWhiteSpace(employeeCode))
                {
                    query = query.Where(e => e.ee.EmployeeCode == employeeCode);
                }

                if (!string.IsNullOrWhiteSpace(location))
                {
                    query = query.Where(e => e.ee.Location != null && e.ee.Location.Contains(location));
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    query = query.Where(e => e.ee.UserName != null && e.ee.UserName.Contains(userName));
                }

                var result = await query.Select(x => new
                {
                    RequestId = x.ee.EdtEmpDetID,
                    OldRecored = new
                    {
                        x.mm.EmployeeCode,
                        x.mm.UserName,
                        x.mm.Gender,
                        Designation = x.mm.Post,
                        x.mm.PositionGrade,
                        Department = x.mm.DeptDFCCIL,
                        x.mm.DOB,
                        DateOfAnniversary = x.mm.AnniversaryDate,
                        DateOfJoining = x.mm.DOJDFCCIL,
                        x.mm.Location,
                        x.mm.Mobile,
                        x.mm.emailAddress,
                        x.mm.PersonalEmailAddress,
                        x.mm.TOemploy,
                        x.mm.AboutUs,
                        x.mm.Photo,
                        x.mm.ReportingOfficer,
                        ExtensionNo = x.mm.ExtnNo,

                    },
                    NewRecords = new
                    {
                        x.ee.EmployeeCode,
                        x.ee.UserName,
                        x.ee.Gender,
                        x.ee.Designation,
                        x.ee.PositionGrade,
                        x.ee.Department,
                        x.ee.DOB,
                        x.ee.DateOfAnniversary,
                        x.ee.DateOfJoining,
                        x.ee.Location,
                        x.ee.Mobile,
                        emailAddress = x.ee.Email,
                        PersonalEmailAddress = x.ee.PersonalEmailId,
                        TOemploy = x.ee.Toemploy,
                        x.ee.AboutUs,
                        x.ee.Photo,
                        x.ee.ReportingOfficer,
                        x.ee.ExtensionNo,
                    },
                }).OrderByDescending(x => x.RequestId).ToListAsync();



                responseModel.Message="Data fetched successfully";
                responseModel.StatusCode=System.Net.HttpStatusCode.OK;
                responseModel.Data = result;
                responseModel.TotalRecords=result.Count();

                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message="Internal Server Error";
                responseModel.StatusCode=System.Net.HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }
        public async Task<ResponseModel> ProcessEditReportingOfficerRequest(AprooveEmployeeReportDto request)
        {
            ResponseModel responseModel = new ResponseModel();

            string employeeCode = request.EmployeeCode;
            bool isApproved = request.IsApproved;
            string remarks = request.Remarks;

            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                responseModel.Message = "EmployeeCode is required.";
                responseModel.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return responseModel;
            }

            if (!isApproved && string.IsNullOrWhiteSpace(remarks))
            {
                responseModel.Message = "Remarks are mandatory when rejecting a request.";
                responseModel.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return responseModel;
            }

            try
            {
                var editRecord = context.EditEmployeeDetails
                    .FirstOrDefault(e => e.EmployeeCode == employeeCode && e.status == 99 && e.TableName == "R");

                if (editRecord == null)
                {
                    responseModel.Message = "No pending edit request found for this employee.";
                    responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return responseModel;
                }

                if (isApproved)
                {
                    var mstRecord = context.MstEmployeeMasters
                        .FirstOrDefault(e => e.EmployeeCode == employeeCode && e.Status == 0);

                    if (mstRecord == null)
                    {
                        responseModel.Message = $"No active master record found for EmployeeCode {employeeCode}.";
                        responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                        return responseModel;
                    }

                    mstRecord.ReportingOfficer = editRecord.ReportingOfficer;

                    editRecord.status = 0;
                    mstRecord.Status = 0;
                    context.SaveChanges();


                    await ApproveKraReportingOfficer(mstRecord.EmployeeMasterAutoId, editRecord.ReportingOfficer);

                    responseModel.Message = "Approved successfully.";
                    responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                    responseModel.Data = new { success = true, ApprovedSuccessfully = true };
                    return responseModel;
                }
                else
                {
                    editRecord.remarks = remarks;
                    editRecord.status = 0;
                    context.SaveChanges();

                    responseModel.Message = "Rejected successfully.";
                    responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                    responseModel.Data = new { success = true, RejectedSuccessfully = true };
                    return responseModel;
                }
            }
            catch (Exception ex)
            {
                responseModel.Message = $"An error occurred: {ex.Message}";
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }

        public async Task<ResponseModel> UpdateDfccilDirectory(UpdateEmployeeDto updateDto)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                var employee = await context.MstEmployeeMasters
                    .FirstOrDefaultAsync(x => x.EmployeeCode.Trim() == updateDto.EmployeeCode.Trim() && x.Status == 0);

                if (employee == null)
                {
                    responseModel.Message = "Employee not found.";
                    responseModel.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return responseModel;
                }


                /*if (!string.IsNullOrEmpty(updateDto.PersonalMobile))*/
                employee.PersonalMobile = updateDto.PersonalMobile;
                /*if (!string.IsNullOrEmpty(updateDto.ExtnNo))*/
                employee.ExtnNo = updateDto.ExtnNo;

                employee.Modify_Date= DateTime.Now;
                employee.Modify_By = updateDto.UpdatedBy;
                await context.SaveChangesAsync();

                responseModel.Message = "DfccilDirectory updated successfully .";
                responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                responseModel.Data = updateDto;

                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = $"An error occurred: {ex.Message}";
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }
        public async Task<ResponseModel> GetAllEmployeeOfTheMonth()
        {
            ResponseModel response = new ResponseModel();

            try
            {
                var records = await context.tblEmployeeOfTheMonths
                      .OrderBy(x => x.status)
                      .OrderByDescending(x => x.yr)
                      .ThenByDescending(x => x.mnth)
                      .Select(e => new
                      {
                          Employee = e,
                          Post = context.MstPosts
                              .Where(p => p.Post.Trim().ToLower() == e.fkEmployeeMasterAuto.Post.Trim().ToLower() && p.Status==0)
                              .FirstOrDefault()
                      })
                      .Select(x => new
                      {
                          unitName = x.Employee.fkEmployeeMasterAuto.Location,
                          EMPCode = x.Employee.fkEmployeeMasterAuto.EmployeeCode,
                          EmployeeName = x.Employee.fkEmployeeMasterAuto.UserName,
                          Designation = x.Employee.fkEmployeeMasterAuto.Post,
                          DesignationDescription = x.Post != null ? x.Post.Description : null,
                          Department = x.Employee.fkEmployeeMasterAuto.DeptDFCCIL,
                          PhotoUrl = !string.IsNullOrEmpty(x.Employee.photo)
                              ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/EmployeeOfTheMonth/{x.Employee.photo}"
                              : $"{baseUrl}/Images/Employees/{x.Employee.fkEmployeeMasterAuto.Photo}",
                          Month = x.Employee.mnth,
                          Year = x.Employee.yr,
                          x.Employee.createDate,
                          x.Employee.createBy
                      })
                      .Skip(1).Take(5)
                    .ToListAsync();

                response.Message = "Employee of the Month records fetched successfully.";
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Data = records;
                response.TotalRecords = records.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> GetCurrentEmployeeOfTheMonth()
        {
            ResponseModel response = new ResponseModel();

            try
            {
                var records = await context.tblEmployeeOfTheMonths
                     .Where(x => x.status == 0)
                     .OrderByDescending(x => x.yr)
                     .ThenByDescending(x => x.mnth)
                     .Select(e => new
                     {
                         Employee = e,
                         Post = context.MstPosts
                             .Where(p => p.Post.Trim().ToLower() == e.fkEmployeeMasterAuto.Post.Trim().ToLower() && p.Status==0)
                             .FirstOrDefault()
                     })
                     .Select(x => new
                     {
                         unitName = x.Employee.fkEmployeeMasterAuto.Location,
                         EMPCode = x.Employee.fkEmployeeMasterAuto.EmployeeCode,
                         EmployeeName = x.Employee.fkEmployeeMasterAuto.UserName,
                         Designation = x.Employee.fkEmployeeMasterAuto.Post,
                         DesignationDescription = x.Post != null ? x.Post.Description : null,
                         Department = x.Employee.fkEmployeeMasterAuto.DeptDFCCIL,
                         PhotoUrl = x.Employee.photo != null
                             ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/EmployeeOfTheMonth/{x.Employee.photo}"
                             : $"{baseUrl}/Images/Employees/{x.Employee.fkEmployeeMasterAuto.Photo}",
                         Month = x.Employee.mnth,
                         Year = x.Employee.yr,
                         x.Employee.createDate,
                         x.Employee.createBy
                     })
                     .FirstOrDefaultAsync();


                if (records == null)
                {
                    response.Message = "No Employee of the Month record found for the current month.";
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return response;
                }

                response.Message = "Employee of the Month record fetched successfully.";
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Data = records;
                response.TotalRecords = 1;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> AddEmployeeOfTheMonth(EmployeeOfTheMonthDto dto)
        {
            var response = new ResponseModel();

            try
            {

                if (dto.Month < 1 || dto.Month > 12)
                {
                    response.Message = "Invalid month. Please provide a month between 1 and 12.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
                var employee = await context.MstEmployeeMasters
                    .FirstOrDefaultAsync(e => e.EmployeeCode == dto.EmpCode && e.Status == 0);

                if (employee == null)
                {
                    return new ResponseModel
                    {
                        Message = "No Employee Found",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }


                var existingRecord = await context.tblEmployeeOfTheMonths
                    .FirstOrDefaultAsync(x => x.mnth == dto.Month && x.yr == dto.Year && x.status==0);


                if (existingRecord != null)
                {
                    existingRecord.fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId;
                    if (dto.photo != null)
                    {
                        var fileName = await UploadPhotoIfAvailable(dto.photo);
                        existingRecord.photo = fileName;
                    }
                    context.SaveChanges();
                    response.Message = "Employee of the Month record updated successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = true;
                    return response;
                }
                else
                {


                    string uploadedFileName = await UploadPhotoIfAvailable(dto.photo);


                    var newEntry = new tblEmployeeOfTheMonth
                    {
                        fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId,
                        mnth = dto.Month,
                        yr = dto.Year,
                        createBy = dto.CreatedBy,
                        createDate = DateTime.Now,
                        photo = uploadedFileName,
                        status = 0
                    };

                    context.tblEmployeeOfTheMonths.Add(newEntry);
                    await context.SaveChangesAsync();

                    response.Message = "Employee of the Month added successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = newEntry.pkId;
                }


            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        #region Notice Board
        public async Task<ResponseModel> GetAllNotices()
        {
            var response = new ResponseModel();
            try
            {
                var records = await context.tblNoticeBoards
                    .Where(n => n.status == 0)
                    .OrderByDescending(n => n.createDate)
                    .Select(x => new
                    {
                        x.pkNoticeId,
                        x.msg,
                        x.doc,
                        x.status,
                        x.createBy,
                        x.createDate,
                        x.subject,
                        x.description,
                        DocUrl = x.doc != null
                            ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/NoticeBoard/{x.doc}"
                            : null
                    })
                    .ToListAsync();

                response.Message = "Notices fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = records;
                response.TotalRecords = records.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> AddNotice(NoticeBoardDto dto)
        {
            var response = new ResponseModel();
            try
            {
                string fileName = string.Empty;
                if (dto.Doc!=null && dto.Doc.Length>0)
                {
                    fileName=   await UploadNoticeDOCIfAvailable(dto.Doc);
                }
                var notice = new tblNoticeBoard
                {
                    msg = dto.Msg,
                    doc = fileName,
                    status = 0,
                    createBy = dto.CreateBy,
                    createDate = DateTime.Now,
                    subject = dto.Subject,
                    description = dto.Description
                };

                await context.tblNoticeBoards.AddAsync(notice);
                await context.SaveChangesAsync();

                response.Message = "Notice added successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = notice;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> UpdateNotice(int id, NoticeBoardDto dto)
        {
            var response = new ResponseModel();
            try
            {
                var notice = await context.tblNoticeBoards.FindAsync(id);

                if (notice == null)
                {
                    response.Message = "Notice not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }
                if (dto.Doc != null && dto.Doc.Length > 0)
                {
                    notice.doc = await UploadNoticeDOCIfAvailable(dto.Doc);
                }
                notice.msg = dto.Msg;
                notice.subject = dto.Subject;
                notice.description = dto.Description;

                await context.SaveChangesAsync();

                response.Message = "Notice updated successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = notice;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> DeleteNotice(int id)
        {
            var response = new ResponseModel();
            try
            {
                var notice = await context.tblNoticeBoards.FindAsync(id);

                if (notice == null)
                {
                    response.Message = "Notice not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }


                notice.status = 9;
                await context.SaveChangesAsync();

                response.Message = "Notice deleted successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = notice;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> GetAllArchiveNotices()
        {
            var response = new ResponseModel();
            try
            {
                var records = await context.tblNoticeBoards
                    .Where(n => n.status == 9)
                    .OrderByDescending(n => n.createDate)
                    .Select(x => new
                    {
                        x.pkNoticeId,
                        x.msg,
                        x.doc,
                        x.status,
                        x.createBy,
                        x.createDate,
                        x.subject,
                        x.description,
                        DocUrl = x.doc != null
                            ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/NoticeBoard/{x.doc}"
                            : null
                    })
                    .ToListAsync();

                response.Message = "Notices fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = records;
                response.TotalRecords = records.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        #endregion

        #region Dependent Methods

        public async Task<ResponseModel> AddDependentsAsync(List<AddDependentDto> dependents, string loginUserEmpCode)
        {
            var response = new ResponseModel();

            if (dependents == null || !dependents.Any())
            {
                response.Message = "No dependent data provided.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(loginUserEmpCode))
            {
                response.Message = "Login user employee code is required.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var firstDependent = dependents.FirstOrDefault();
                if (firstDependent == null || string.IsNullOrWhiteSpace(firstDependent.EmployeeCode))
                {
                    response.Message = "EmployeeCode is required.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                string empCode = firstDependent.EmployeeCode.Trim();

                if (dependents.Any(d => string.IsNullOrWhiteSpace(d.EmployeeCode) || d.EmployeeCode.Trim() != empCode))
                {
                    response.Message = "All dependents must belong to the same employee.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var employee = await context.MstEmployeeMasters
                    .FirstOrDefaultAsync(e => e.EmployeeCode == empCode && e.Status == 0);

                if (employee == null)
                {
                    response.Message = "Employee not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                long fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId;

                var existingPendingRequest = await context.MstEmployeeDependents
                    .AnyAsync(x => x.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId && x.status == 99);

                if (existingPendingRequest)
                {
                    response.Message = "There is already a pending request for this employee.";
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                var existingDependents = await context.MstEmployeeDependents
                    .Where(x => x.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId && x.status == 0)
                    .Select(x => new { x.DName, x.Relation })
                    .ToListAsync();

                var validationErrors = new List<string>();
                var duplicateCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < dependents.Count; i++)
                {
                    var dto = dependents[i];

                    try
                    {
                        if (string.IsNullOrWhiteSpace(dto.Relation) ||
                            string.IsNullOrWhiteSpace(dto.DName) ||
                            string.IsNullOrWhiteSpace(dto.Gender) ||
                            dto.Age <= 0)
                        {
                            validationErrors.Add($"Dependent #{i + 1} '{dto?.DName ?? "Unknown"}': All fields are required and age must be positive.");
                            continue;
                        }

                        string sanitizedName = dto.DName.Trim();
                        string sanitizedRelation = dto.Relation.Trim();

                        string dependentKey = $"{sanitizedName}_{sanitizedRelation}";
                        if (!duplicateCheck.Add(dependentKey))
                        {
                            validationErrors.Add($"Duplicate dependent in request: '{sanitizedName}' with relation '{sanitizedRelation}'.");
                            continue;
                        }

                        bool existsInDb = existingDependents.Any(x =>
                            string.Equals(x.DName, sanitizedName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.Relation, sanitizedRelation, StringComparison.OrdinalIgnoreCase));

                        if (existsInDb)
                        {
                            validationErrors.Add($"Dependent '{sanitizedName}' with relation '{sanitizedRelation}' already exists.");
                        }

                        if (dto.Age > 120)
                        {
                            validationErrors.Add($"Dependent '{sanitizedName}': Age cannot exceed 100 years.");
                        }

                        if (sanitizedName.Length > 100)
                        {
                            validationErrors.Add($"Dependent name '{sanitizedName}' is too long (max 100 characters).");
                        }
                    }
                    catch (Exception validationEx)
                    {
                        validationErrors.Add($"Error validating dependent #{i + 1}: {validationEx.Message}");
                    }
                }

                if (validationErrors.Any())
                {
                    response.Message = "Validation failed: " + string.Join("; ", validationErrors);
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }


                var strategy = context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await context.Database.BeginTransactionAsync();
                    try
                    {
                        var addedDependents = new List<MstEmployeeDependent>();
                        var currentDateTime = DateTime.Now;

                        foreach (var dto in dependents)
                        {
                            var newDependent = new MstEmployeeDependent
                            {
                                fkEmployeeMasterAutoId = fkEmployeeMasterAutoId,
                                Relation = dto.Relation.Trim(),
                                DName = dto.DName.Trim(),
                                Gender = dto.Gender.Trim(),
                                Age = dto.Age,
                                status = 99,
                                createdBy = loginUserEmpCode?.Trim(),
                                createdDate = currentDateTime.Date
                            };

                            context.MstEmployeeDependents.Add(newDependent);
                            addedDependents.Add(newDependent);
                        }

                        await context.SaveChangesAsync();

                        var successfulUploads = 0;
                        var failedUploads = new List<string>();

                        for (int i = 0; i < dependents.Count; i++)
                        {
                            var dto = dependents[i];
                            var dependent = addedDependents[i];

                            if (dto.DocumentFiles != null && dto.DocumentFiles.Any())
                            {
                                try
                                {
                                    await UploadDependentDOCIfAvailable(dto.DocumentFiles, dependent.pkDependentId, loginUserEmpCode);
                                    successfulUploads++;
                                }
                                catch (Exception uploadEx)
                                {
                                    failedUploads.Add($"Failed to upload documents for '{dto.DName}': {uploadEx.Message}");
                                }
                            }
                        }

                        await transaction.CommitAsync();

                        var responseMessage = $"Successfully added {addedDependents.Count} dependent(s) for employee {empCode}.";
                        if (failedUploads.Any())
                        {
                            responseMessage += $" Note: {failedUploads.Count} document upload(s) failed.";
                        }

                        response.Message = responseMessage;
                        response.StatusCode = HttpStatusCode.Created;
                        response.Data = new
                        {
                            EmployeeCode = empCode,
                            DependentsAdded = addedDependents.Count,
                            DependentIds = addedDependents.Select(d => d.pkDependentId).ToList(),
                            DocumentUploads = new
                            {
                                Successful = successfulUploads,
                                Failed = failedUploads.Count,
                                Errors = failedUploads
                            }
                        };
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (ArgumentNullException argEx)
            {
                response.Message = $"Invalid argument: {argEx.Message}";
                response.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (InvalidOperationException opEx)
            {
                response.Message = $"Operation error: {opEx.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (SqlException sqlEx)
            {
                response.Message = $"Database error: {sqlEx.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (DbUpdateException dbEx)
            {
                response.Message = $"Database update error: {dbEx.InnerException?.Message ?? dbEx.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                response.Message = $"An unexpected error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> ProceedDependentsAsync(AprooveEmployeeReportDto request, string LoginUserEmpCode)
        {
            var response = new ResponseModel();

            try
            {
                string employeeCode = request.EmployeeCode;
                bool isApproved = request.IsApproved;
                string remarks = request.Remarks;

                if (string.IsNullOrWhiteSpace(employeeCode))
                {
                    response.Message = "EmployeeCode is required.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }


                if (isApproved == false && string.IsNullOrWhiteSpace(remarks))
                {
                    response.Message = "Remarks are mandatory when rejecting a request.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
                var employeeCodeTrimmed = employeeCode.Trim();
                var employee = context.MstEmployeeMasters
                    .FirstOrDefault(e => e.EmployeeCode.Trim() == employeeCodeTrimmed && e.Status == 0);
                if (employee == null)
                {
                    response.Message = "Employee not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                var editRecord = await context.MstEmployeeDependents
                .Where(e => e.fkEmployeeMasterAutoId == employee.EmployeeMasterAutoId && e.status == 99).ToListAsync();

                if (editRecord.Count == 0)
                {
                    response.Message = "No pending edit request found for this employee.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                if (isApproved)
                {
                    foreach (var item in editRecord)
                    {
                        item.updatedDate=DateTime.Now.Date;
                        item.updatedBy =LoginUserEmpCode;
                        item.status = 0;
                        context.SaveChanges();
                    }



                    response.Message = "Approved successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = new { success = true, ApprovedSuccessfully = true };
                    return response;
                }
                else
                {
                    foreach (var item in editRecord)
                    {
                        item.updatedDate=DateTime.Now.Date;
                        item.updatedBy =LoginUserEmpCode;
                        item.remarks = remarks;
                        item.status = 9;
                        context.SaveChanges();
                    }


                    response.Message = "Rejected successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = new { success = true, RejectedSuccessfully = true };
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }
        public async Task<ResponseModel> GetAllDependentsRequestByEmpCodeAsync()
        {
            var response = new ResponseModel();
            try
            {

                var dependentsWithEmployees = await context.MstEmployeeDependents
                    .Where(dep => dep.status == 99)
                    .Include(dep => dep.EmployeeDependentDocuments)
                    .Join(context.MstEmployeeMasters,
                        dep => dep.fkEmployeeMasterAutoId,
                        emp => emp.EmployeeMasterAutoId,
                        (dep, emp) => new
                        {
                            dep.pkDependentId,
                            emp.EmployeeCode,
                            dep.DName,
                            dep.Relation,
                            dep.Gender,
                            dep.Age,
                            dep.status,
                            dep.EmployeeDependentDocuments
                        })
                    .ToListAsync();


                var groupedDependents = dependentsWithEmployees
                    .GroupBy(x => x.EmployeeCode)
                    .Select(g => new
                    {
                        EmployeeCode = g.Key,
                        Dependents = g.Select(d => new
                        {
                            d.pkDependentId,
                            d.DName,
                            d.Relation,
                            d.Gender,
                            d.Age,
                            d.status,
                            DocumentList = d.EmployeeDependentDocuments?.Select(doc => new
                            {
                                doc.DocumentId,
                                filePath = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/DependentDocuments/{doc.DocumentName}",
                                doc.DocumentName,
                                doc.DocumentType,
                                doc.Remarks
                            }).ToList()
                        }).ToList()
                    })
                    .ToList();

                response.Message = "Grouped dependents fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = groupedDependents;
                response.TotalRecords = groupedDependents.Count;
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred: " + ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }
        public async Task<ResponseModel> GetDependentsByEmpCodeAsync(string empCode)
        {
            var response = new ResponseModel();

            if (string.IsNullOrWhiteSpace(empCode))
            {
                response.Message = "EmployeeCode is required.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                var employee = context.MstEmployeeMasters
                    .FirstOrDefault(e => e.EmployeeCode == empCode && e.Status == 0);

                if (employee == null)
                {
                    response.Message = "Employee not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                long fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId;

                var dependentsWithEmployees = await context.MstEmployeeDependents
                   .Where(dep => dep.status == 99 && dep.fkEmployeeMasterAutoId==fkEmployeeMasterAutoId)
                   .Include(dep => dep.EmployeeDependentDocuments)
                   .Join(context.MstEmployeeMasters,
                       dep => dep.fkEmployeeMasterAutoId,
                       emp => emp.EmployeeMasterAutoId,
                       (dep, emp) => new
                       {
                           dep.pkDependentId,
                           emp.EmployeeCode,
                           dep.DName,
                           dep.Relation,
                           dep.Gender,
                           dep.Age,
                           dep.status,
                           dep.EmployeeDependentDocuments
                       })
                   .ToListAsync();


                var groupedDependents = dependentsWithEmployees
                    .GroupBy(x => x.EmployeeCode)
                    .Select(g => new
                    {
                        EmployeeCode = g.Key,
                        Dependents = g.Select(d => new
                        {
                            d.pkDependentId,
                            d.DName,
                            d.Relation,
                            d.Gender,
                            d.Age,
                            d.status,
                            DocumentList = d.EmployeeDependentDocuments?.Select(doc => new
                            {
                                doc.DocumentId,
                                filePath = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/DependentDocuments/{doc.DocumentName}",
                                doc.DocumentName,
                                doc.DocumentType,
                                doc.Remarks
                            }).ToList()
                        }).ToList()
                    })
                    .FirstOrDefault();

                response.Message = "Dependents fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = groupedDependents?? new object();
                response.TotalRecords = 1;
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred: " + ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> GetDependentsListByEmpCodeAsync(string empCode)
        {
            var response = new ResponseModel();

            if (string.IsNullOrWhiteSpace(empCode))
            {
                response.Message = "EmployeeCode is required.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {

                var employee = await context.MstEmployeeMasters
                    .FirstOrDefaultAsync(e => e.EmployeeCode == empCode && e.Status == 0);

                if (employee == null)
                {
                    response.Message = "Employee not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                long fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId;


                var dependentsWithEmployees = await context.MstEmployeeDependents
                    .Where(dep => dep.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId && dep.status == 0)
                    .Include(dep => dep.EmployeeDependentDocuments)
                    .Join(context.MstEmployeeMasters,
                        dep => dep.fkEmployeeMasterAutoId,
                        emp => emp.EmployeeMasterAutoId,
                        (dep, emp) => new
                        {
                            dep.pkDependentId,
                            emp.EmployeeCode,
                            dep.DName,
                            dep.Relation,
                            dep.Gender,
                            dep.Age,
                            dep.status,
                            dep.EmployeeDependentDocuments
                        })
                    .ToListAsync();

                if (!dependentsWithEmployees.Any())
                {
                    response.Message = "No dependents found for this employee.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Data = new { EmployeeCode = empCode, Dependents = new List<object>() };
                    response.TotalRecords = 0;
                    return response;
                }


                var groupedDependents = dependentsWithEmployees
                    .GroupBy(x => x.EmployeeCode)
                    .Select(g => new
                    {
                        EmployeeCode = g.Key,
                        TotalDependents = g.Count(),
                        Dependents = g.Select(d => new
                        {
                            d.pkDependentId,
                            d.DName,
                            d.Relation,
                            d.Gender,
                            d.Age,
                            d.status,
                            DocumentCount = d.EmployeeDependentDocuments?.Count ?? 0,
                            DocumentList = d.EmployeeDependentDocuments?.Select(doc => new
                            {
                                doc.DocumentId,
                                filePath = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/DependentDocuments/{doc.DocumentName}",
                                doc.DocumentName,
                                doc.DocumentType,
                                doc.Remarks
                            }).ToList()
                        }).ToList()
                    })
                    .FirstOrDefault();

                response.Message = $"Dependents fetched successfully. Found {groupedDependents?.Dependents?.Count ?? 0} dependents.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = groupedDependents?? new object();
                response.TotalRecords = groupedDependents?.Dependents?.Count ?? 0;
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred: " + ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> UpdateDependentAsync(int DependentId, AddDependentDto dto, string loginUserEmpCode)
        {
            var response = new ResponseModel();

            if (dto == null)
            {
                response.Message = "Dependent data is required.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (DependentId <= 0)
            {
                response.Message = "Dependent ID is required for update.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
            {
                response.Message = "EmployeeCode is required.";
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            try
            {
                string empCode = dto.EmployeeCode.Trim();
                var employee = await context.MstEmployeeMasters
                    .FirstOrDefaultAsync(e => e.EmployeeCode == empCode && e.Status == 0);

                if (employee == null)
                {
                    response.Message = "Employee not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                long fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId;

                var existingDependent = await context.MstEmployeeDependents
                    .FirstOrDefaultAsync(x => x.pkDependentId == DependentId &&
                                              x.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId);

                if (existingDependent == null)
                {
                    response.Message = $"Dependent with ID {DependentId} not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }




                existingDependent.Relation = string.IsNullOrWhiteSpace(dto.Relation) ? existingDependent.Relation : dto.Relation.Trim();
                existingDependent.DName = string.IsNullOrWhiteSpace(dto.DName) ? existingDependent.DName : dto.DName.Trim();
                existingDependent.Gender = string.IsNullOrWhiteSpace(dto.Gender) ? existingDependent.Gender : dto.Gender.Trim();
                if (dto.Age > 0) existingDependent.Age = dto.Age;


                existingDependent.status = 99;
                existingDependent.updatedBy = loginUserEmpCode;
                existingDependent.updatedDate = DateTime.Now.Date;

                context.MstEmployeeDependents.Update(existingDependent);
                await context.SaveChangesAsync();


                if (dto.DocumentFiles != null && dto.DocumentFiles.Any())
                {
                    await UploadDependentDOCIfAvailable(dto.DocumentFiles, existingDependent.pkDependentId, loginUserEmpCode, "UPDATE");
                }

                response.Message = "Dependent updated successfully.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred: " + ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        #endregion

        #region Aprrove Contractual Employee 
        public async Task<ResponseModel> ProcessEditContractualEmployeeRequest(AprooveContractualEmployeeDto request, string LoginUserId)
        {
            var response = new ResponseModel();
            var ApproveTempalteId = configuration["ContractualAprrovedSMSTempleteId"]?? string.Empty;
            var RejectTempalteId = configuration["ContractualRejectedSMSTempleteId"]?? string.Empty;

            try
            {
                if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Remarks))
                {
                    response.Message = "Remarks are mandatory when rejecting a request.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                var existingEmployee = await context.RegisterContractEmployees
                    .FirstOrDefaultAsync(e => e.ContEmpID == request.ContraualEmployeeRequestId && e.Status == 8);

                if (existingEmployee == null)
                {
                    response.Message = "Employee Request not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }
                string basePath = string.Empty;
                if (configuration["DeploymentModes"]=="DFCCIL_UAT")
                {
                    basePath = configuration["ContractEmployeeDocumentPathUat"] ?? string.Empty;
                }
                else
                {
                    basePath = configuration["ContractEmployeeDocumentPathProd"] ?? string.Empty;
                }

                var oldFileName = existingEmployee.AppointmentDoc ?? string.Empty;
                var oldFilePath = Path.Combine(basePath, oldFileName);

                if (request.IsApproved)
                {

                    var latestEmpCode = context.MstEmployeeMasters
                        .Where(x => x.TOemploy.ToLower().Contains("contract"))
                        .OrderByDescending(x => x.EmployeeCode)
                        .Select(x => x.EmployeeCode)
                        .FirstOrDefault();

                    var newEmpCode = (Convert.ToInt32(latestEmpCode) + 1).ToString();


                    existingEmployee.Status = 0;
                    existingEmployee.UpdatedBy = LoginUserId;
                    existingEmployee.UpdatedDate = DateTime.Now;


                    var mstEmployee = new MstEmployeeMaster
                    {
                        EmployeeCode = newEmpCode,
                        UserName = existingEmployee.UserName,
                        DeptDFCCIL = existingEmployee.DeptDFCCIL,
                        Location = existingEmployee.Location,
                        Mobile = existingEmployee.Mobile,
                        emailAddress = existingEmployee.emailAddress,
                        TOemploy = existingEmployee.TOemploy,
                        Gender = existingEmployee.Gender,
                        DOB = existingEmployee.DOB,
                        DOJDFCCIL = existingEmployee.DOJDFCCIL,
                        Status = 0,
                        Modify_Date = DateTime.Now,
                        Modify_By = LoginUserId
                    };

                    context.MstEmployeeMasters.Add(mstEmployee);
                    await context.SaveChangesAsync();

                    await Send2SmsAsync(ApproveTempalteId, existingEmployee.UserName, existingEmployee.Mobile, newEmpCode);

                    var mstContract = new MstContractEmployeeMaster
                    {
                        fkEmployeeMasterAutoId = mstEmployee.EmployeeMasterAutoId,
                        fkContractid = existingEmployee.fkContractid,
                        status = 0,
                        Remarks = existingEmployee.remarks,
                        OfficeOrder = oldFileName
                    };


                    if (File.Exists(oldFilePath))
                    {
                        var newFileName = Regex.Replace(oldFileName, @"_(\d+)(\.pdf)$", $"_{newEmpCode}$2");
                        var newFilePath = Path.Combine(basePath, newFileName);

                        File.Move(oldFilePath, newFilePath);

                        mstContract.OfficeOrder = newFileName;
                        existingEmployee.AppointmentDoc = newFileName;
                        Console.WriteLine("File renamed successfully.");
                    }

                    context.MstContractEmployeeMasters.Add(mstContract);
                }
                else
                {

                    existingEmployee.Status = 9;
                    existingEmployee.remarks = request.Remarks;

                    var mstContract = new MstContractEmployeeMaster
                    {
                        fkEmployeeMasterAutoId = existingEmployee.ContEmpID,
                        fkContractid = existingEmployee.fkContractid,
                        status = 9,
                        Remarks = existingEmployee.remarks,
                        OfficeOrder = existingEmployee.AppointmentDoc
                    };

                    context.MstContractEmployeeMasters.Add(mstContract);

                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                        Console.WriteLine("File deleted successfully.");
                    }
                    await Send2SmsAsync(RejectTempalteId, existingEmployee.UserName, existingEmployee.Mobile);
                }

                await context.SaveChangesAsync();






                response.Message = $"Request {(request.IsApproved ? "Approved" : "Rejected")} Successfully.";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }
        public async Task<ResponseModel> GetAllContractualEmployeeEditRequestsAsync()
        {
            var response = new ResponseModel();


            try
            {
                var requests = await context.RegisterContractEmployees
                    .Where(e => e.Status == 8)
                    .Select(e => new
                    {
                        e.ContEmpID,
                        e.UserName,
                        e.DeptDFCCIL,
                        e.Location,
                        e.Mobile,
                        e.emailAddress,
                        e.TOemploy,
                        e.Gender,
                        e.DOB,
                        e.DOJDFCCIL,
                        e.Status,
                        e.CreateDate,
                        e.UpdatedDate,
                        e.UpdatedBy,
                        AppointmentDoc = e.AppointmentDoc!=null ? $"{baseUrl}/DocUpload/OfficeOrder/{e.AppointmentDoc}" : null
                    })
                    .ToListAsync();

                response.Message = "Contratual Employee requests fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = requests;
                response.TotalRecords = requests.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> GetAcceptOrRejectContractualEmployeeEditRequestsAsync(int status)
        {
            var response = new ResponseModel();


            try
            {
                var requests = await context.RegisterContractEmployees
                    .Where(e => e.Status == status)
                    .Select(e => new
                    {
                        e.ContEmpID,
                        e.UserName,
                        e.DeptDFCCIL,
                        e.Location,
                        e.Mobile,
                        e.emailAddress,
                        e.TOemploy,
                        e.Gender,
                        e.DOB,
                        e.DOJDFCCIL,
                        e.Status,
                        e.CreateDate,
                        e.UpdatedDate,
                        e.UpdatedBy,
                        AppointmentDoc = e.AppointmentDoc!=null ? $"{baseUrl}/DocUpload/OfficeOrder/{e.AppointmentDoc}" : null
                    })
                    .ToListAsync();

                response.Message = "Contratual Employee requests fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = requests;
                response.TotalRecords = requests.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        #endregion
        public ResponseModel GetEditEmployeeStatus(string EmployeeCode)
        {
            var response = new ResponseModel();

            try
            {
                var result = GETEditEmployeeStatus(EmployeeCode);

                if (result != null && result.Data is DataTable dt && dt.Rows.Count > 0)
                {

                    var dataList = dt.AsEnumerable()
                        .Select(row => dt.Columns.Cast<DataColumn>()
                            .ToDictionary(col => col.ColumnName, col => row[col]))
                        .ToList();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Edit Employee Status fetched successfully.";
                    response.Data = dataList;
                    response.TotalRecords = dt.Rows.Count;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "No employee status found.";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while fetching edit employee status.";
                response.Error = true;
                response.ErrorDetail = ex;
            }

            return response;
        }

        #region Notifiaction Methods
        public async Task<PagedResponseModel> GetSMSLogDetailsPaginatedAsync(SMSLogRequest request)
        {
            var response = new PagedResponseModel();
            try
            {
                using var connection = dapper.GetConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@EmpCode", request.EmpCode);
                parameters.Add("@PageNumber", request.PageNumber);
                parameters.Add("@PageSize", request.PageSize);
                parameters.Add("@Status", request.Status);
                parameters.Add("@DateFrom", request.DateFrom);
                parameters.Add("@DateTo", request.DateTo);
                parameters.Add("@SearchText", request.SearchText);

                var results = await connection.QueryAsync<SMSLogDetailDto>(
                    "[GetSmsNotificationByUser]",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 30
                );

                var smsLogs = results.Select(x => new
                {
                    x.SMSSentId,
                    x.MobileNumber,
                    x.SMSText,
                    x.SentOn,
                    x.UserId,
                    x.IsRead,
                    x.NotificationType
                }).ToList();

                if (!smsLogs.Any())
                {
                    response.Message = "No SMS logs found for the given criteria.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Data = new object();
                    response.TotalRecords = 0;
                    response.CurrentPage = request.PageNumber;
                    response.PageSize = request.PageSize;
                    response.TotalPages = 0;
                    return response;
                }


                var firstRecord = results.First();

                response.Data = smsLogs;
                response.TotalRecords = firstRecord.TotalRecords;
                response.CurrentPage = firstRecord.CurrentPage;
                response.PageSize = firstRecord.PageSize;
                response.TotalPages = firstRecord.TotalPages;
                response.Message = $"Found {smsLogs.Count} SMS logs on page {response.CurrentPage} of {response.TotalPages}.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (SqlException sqlEx)
            {
                response.Message = $"Database error: {sqlEx.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
        public async Task<ResponseModel> UpdateSMSAsync(int SmsId)
        {
            var response = new ResponseModel();
            try
            {
                var existingSMS = await context.SMSLogDetails
                    .FirstOrDefaultAsync(x => x.SMSSentId == SmsId);

                if (existingSMS == null)
                {
                    response.Message = $"sms with ID {SmsId} not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }


                existingSMS.ArchiveStatus=true;
                await context.SaveChangesAsync();




                response.Message = "sms updated successfully.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred: " + ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        #endregion

        #region TODO List
        public async Task<ResponseModel> CreateToDoListAsync(CreateTodoListDto dto)
        {
            var response = new ResponseModel();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Empcode))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Invalid request: Employee code is required.";
                return response;
            }

            try
            {
                var existingTodo = await context.todoLists
                    .FirstOrDefaultAsync(x => x.EmployeeCode == dto.Empcode);

                if (existingTodo != null)
                {
                    existingTodo.data = dto.Data;
                    await context.SaveChangesAsync();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Todo updated successfully.";
                    response.Data = existingTodo;
                    return response;
                }

                var employeeAutoId = await context.MstEmployeeMasters
                    .Where(x => x.EmployeeCode.Trim() == dto.Empcode.Trim() && x.Status == 0)
                    .Select(x => x.EmployeeMasterAutoId)
                    .FirstOrDefaultAsync();

                if (employeeAutoId == 0)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = $"No active employee found with the code '{dto.Empcode}'.";
                    return response;
                }

                var todo = new todoList
                {
                    EmployeeCode = dto.Empcode,
                    data = dto.Data,
                    fkEmpId = employeeAutoId,
                    createDate = DateTime.UtcNow,
                    createdBy = dto.Empcode,
                    status = 0
                };

                await context.todoLists.AddAsync(todo);
                await context.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.Created;
                response.Message = "Todo created successfully.";
                response.Data = todo.pkToDoListId;

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        public async Task<ResponseModel> GetToDoListAsync(string employeeCode = "0")
        {
            var response = new ResponseModel();

            try
            {
                if (string.IsNullOrWhiteSpace(employeeCode) || employeeCode == "0")
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "Employee code is required.";
                    return response;
                }

                var todoList = await context.todoLists
                    .Where(x => x.EmployeeCode == employeeCode && x.status == 0)
                    .Select(x => new
                    {
                        x.pkToDoListId,
                        x.EmployeeCode,
                        x.data,
                        x.createDate,
                        x.createdBy
                    })
                    .ToListAsync();

                if (todoList == null || !todoList.Any())
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = $"No Todo list found for EmployeeCode: {employeeCode}";
                    response.Data = DateTime.UtcNow;
                    return response;
                }

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Success";
                response.Data = todoList;
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"Error: {ex.Message}";
                response.Data = DateTime.UtcNow;
                return response;
            }
        }

        #endregion
        #region HelperMethods
        private async Task<string> UploadPhotoIfAvailable(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return null;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmployeeOftheMonth");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return fileName;
        }
        private ResponseModel GETEditEmployeeStatus(string EmployeeCode)
        {
            var response = new ResponseModel();
            var ds = new DataSet();

            try
            {
                using var connection = dapper.GetConnection();
                using var adapter = new SqlDataAdapter("[DFCAPI].GetEditEmployeeStatus", connection)
                {
                    SelectCommand = { CommandType = CommandType.StoredProcedure }
                };
                adapter.SelectCommand.Parameters.AddWithValue("@EmpCode", EmployeeCode ?? "X");
                adapter.Fill(ds);

                response.StatusCode = HttpStatusCode.OK;
                response.Data = ds.Tables.Count > 0 ? ds.Tables[0] : null;
                response.TotalRecords = ds.Tables.Count > 0 ? ds.Tables[0].Rows.Count : 0;
                response.Message = "Edit Employee Status fetched successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Error fetching edit employee status.";
                response.Error = true;
                response.ErrorDetail = ex.Message;
            }

            return response;
        }
        private async Task<string> UploadNoticeDOCIfAvailable(IFormFile doc)
        {
            if (doc == null || doc.Length == 0)
                return null;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NoticeBoard");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(doc.FileName)}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await doc.CopyToAsync(stream);
            }

            return fileName;
        }
        private async Task UploadDependentDOCIfAvailable(List<DependtentsDocuments> docs, long fkMstDependentId, string loginUserId, string Flag = "")
        {
            if (docs == null || !docs.Any())
                return;

            var validDocs = docs
                .Where(d => d.DocumentFile != null && d.DocumentFile.Length > 0)
                .ToList();

            if (!validDocs.Any())
                return;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DependentDocuments");
            Directory.CreateDirectory(uploadsFolder);

            var documentEntities = new List<EmployeeDependentDocument>();

            foreach (var doc in validDocs)
            {
                string extension = Path.GetExtension(doc?.DocumentFile?.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                    continue;

                string fileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doc?.DocumentFile?.CopyToAsync(stream);
                }
                if (Flag=="UPDATE")
                {
                    var existingDoc = await context.EmployeeDependentDocuments
                        .Where(x => x.fkDependentId == fkMstDependentId).ToListAsync();
                    existingDoc.ForEach(x => x.Status=9);
                    context.EmployeeDependentDocuments.UpdateRange(existingDoc);
                }
                documentEntities.Add(new EmployeeDependentDocument
                {
                    fkDependentId = fkMstDependentId,
                    DocumentName = fileName,
                    DocumentType = doc.DocumentType?.Trim(),
                    FilePath = filePath,
                    UploadedBy = loginUserId,
                    UploadedDate = DateTime.Now.Date,
                    Remarks = doc.Remarks?.Trim(),
                    Status = 0
                });
            }

            if (documentEntities.Any())
            {
                await context.EmployeeDependentDocuments.AddRangeAsync(documentEntities);
                await context.SaveChangesAsync();
            }
        }
        private async Task ApproveKraReportingOfficer(long employeeAutoId, string reportingOfficerEmpCode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@EmpAutoId", employeeAutoId);
            parameters.Add("@ReportingOfficerEmpCode", reportingOfficerEmpCode);

            await dapper.ExecuteAsync(
                "[DFCAPI].Sp_Approved_Reporting_Officer",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var employeeTempalteId = configuration["EmployeeSMSTempleteId"]?? string.Empty;
            var ReportingOfficerTemplateId = configuration["ReportingOfficerSMSTempleteId"]?? string.Empty;

            var EmpCode = await context.MstEmployeeMasters.FirstOrDefaultAsync(x => x.Status==0 && x.EmployeeMasterAutoId==employeeAutoId);
            if (EmpCode!=null)
            {
                await SendSmsAsync(ReportingOfficerTemplateId, reportingOfficerEmpCode, EmpCode.UserName);
                await SendSmsAsync(employeeTempalteId, EmpCode.EmployeeCode);
            }
        }
        private async Task<ResponseModel> SendSmsAsync(string templateId, string empCode, string userName = "")
        {
            var response = new ResponseModel();

            try
            {

                var employee = context.MstEmployeeMasters
                    .FirstOrDefault(x => x.Status == 0 && x.EmployeeCode == empCode);

                if (employee == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Employee not found.";
                    return response;
                }


                string username = configuration["SMSServiceUserName"] ?? string.Empty;
                string phone = string.Empty;
                if (configuration["DeploymentModes"] !="DFCCIL")
                {
                    phone = configuration["SMSServiceDefaultNumber"]?? string.Empty;
                }
                else
                {
                    phone = employee.Mobile;
                }

                string msg;

                if (configuration["EmployeeSMSTempleteId"] == templateId)
                {
                    msg = $"Dear {employee.UserName}, The request for profile update on the DFC portal has been approved. KRA for the current year may be reprocessed for approval. From DFCCIL.";
                }
                else
                {
                    msg = $"Dear {employee.UserName}, On DFC Portal, {userName} has selected you as Reporting Officer. Kindly Review and approve the KRA. From DFCCIL.";
                }


                string encodedMsg = Uri.EscapeDataString(msg);

                if (string.IsNullOrEmpty(phone))
                {
                    return new ResponseModel();
                }
                string contextUrl = $"https://login.dfccil.com/Dfccil/DFCSMS?username={Uri.EscapeDataString(username)}&Phone={Uri.EscapeDataString(phone)}&msg={encodedMsg}&templatedid={Uri.EscapeDataString(templateId)}";

                using var httpClient = new HttpClient();
                var contextResponse = await httpClient.PostAsync(contextUrl, null);

                if (contextResponse.IsSuccessStatusCode)
                {
                    var resultContent = await contextResponse.Content.ReadAsStringAsync();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "SMS sent successfully.";
                    response.Data = resultContent;
                    response.TotalRecords = 1;
                }
                else
                {
                    var errorContent = await contextResponse.Content.ReadAsStringAsync();
                    response.StatusCode = contextResponse.StatusCode;
                    response.Message = $"Failed to send SMS. Status Code: {contextResponse.StatusCode}";
                    response.Error = true;
                    response.ErrorDetail = errorContent;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while sending SMS.";
                response.Error = true;
                response.ErrorDetail = ex;
            }

            return response;
        }
        private async Task<ResponseModel> Send2SmsAsync(string templateId, string username, string mobile, string EmpCode = "")
        {
            var response = new ResponseModel();
            string Username = configuration["SMSServiceUserName"] ?? string.Empty;
            string phone = string.Empty;

            try
            {
                string msg;

                if (configuration["ContractualAprrovedSMSTempleteId"] == templateId)
                {
                    if (configuration["DeploymentModes"] !="DFCCIL")
                    {
                        phone = configuration["SMSServiceDefaultNumber"]?? string.Empty;
                    }
                    else
                    {
                        phone = mobile;
                    }
                    msg = string.Format("Dear " + username + ", your request for registration on IT connect portal has been approved. Your employee ID is - " + EmpCode + ". From DFCCIL.");
                }
                else
                {
                    if (configuration["DeploymentModes"] !="DFCCIL")
                    {
                        phone = configuration["SMSServiceDefaultNumber"]?? string.Empty;
                    }
                    else
                    {
                        phone =mobile;
                    }
                    msg = string.Format("Dear " + username + ", your request for registration on IT connect portal has been rejected. From DFCCIL");
                }


                string encodedMsg = Uri.EscapeDataString(msg);

                if (string.IsNullOrEmpty(phone))
                {
                    return new ResponseModel();
                }
                string contextUrl = $"https://login.dfccil.com/Dfccil/DFCSMS?username={Uri.EscapeDataString(Username)}&Phone={Uri.EscapeDataString(phone)}&msg={encodedMsg}&templatedid={Uri.EscapeDataString(templateId)}";

                using var httpClient = new HttpClient();
                var contextResponse = await httpClient.PostAsync(contextUrl, null);

                if (contextResponse.IsSuccessStatusCode)
                {
                    var resultContent = await contextResponse.Content.ReadAsStringAsync();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "SMS sent successfully.";
                    response.Data = resultContent;
                    response.TotalRecords = 1;
                }
                else
                {
                    var errorContent = await contextResponse.Content.ReadAsStringAsync();
                    response.StatusCode = contextResponse.StatusCode;
                    response.Message = $"Failed to send SMS. Status Code: {contextResponse.StatusCode}";
                    response.Error = true;
                    response.ErrorDetail = errorContent;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while sending SMS.";
                response.Error = true;
                response.ErrorDetail = ex;
            }

            return response;
        }
        #endregion

        public async Task<ResponseModel> GetDfccilDirectory(string? EmpCode = null)
        {
            try
            {
                var cacheKey = $"dfccil_directory_{EmpCode ?? "all"}";
                var versionCacheKey = $"{cacheKey}_version";


                var currentDataVersion = await _dbChangeService.GetDataVersionAsync();


                var cachedVersion = await _cacheService.GetAsync<long?>(versionCacheKey);

                if (cachedVersion == null || cachedVersion != currentDataVersion)
                {

                    await _cacheService.RemoveAsync(cacheKey);


                    await _cacheService.SetAsync(versionCacheKey, currentDataVersion, TimeSpan.FromHours(2));

                    _logger.LogInformation("Data version changed, cache invalidated for {CacheKey}", cacheKey);
                }


                var responseModel = await _cacheService.GetOrSetAsync(
                    cacheKey,
                    async () => await FetchDirectoryFromDatabase(EmpCode),
                    TimeSpan.FromMinutes(30),
                    TimeSpan.FromHours(2)
                );

                return responseModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDfccilDirectory");
                return new ResponseModel
                {
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }


        private async Task<ResponseModel> FetchDirectoryFromDatabase(string? EmpCode)
        {
            _logger.LogInformation("Fetching directory data from database for EmpCode: {EmpCode}", EmpCode ?? "all");

            var responseModel = new ResponseModel();

            try
            {
                var employeeMaster = await context.MstEmployeeMasters
                    .Where(x => (EmpCode == null || x.EmployeeCode == EmpCode) && x.Status == 0)
                    .GroupJoin(
                        context.mstPositionGreades,
                        x => x.PositionGrade,
                        y => y.PositionGrade,
                        (x, y) => new { x, y }
                    )
                    .SelectMany(
                        xy => xy.y.DefaultIfEmpty(),
                        (xy, y) => new
                        {
                            EmpCode = xy.x.EmployeeCode,
                            EmployeeType = xy.x.TOemploy.ToUpper(),
                            PositionGrade = xy.x != null ? xy.x.PositionGrade : null,
                            OfficalMobile = xy.x.Mobile,
                            name = xy.x.UserName,
                            unit = xy.x.Location,
                            personalMobile = xy.x.PersonalMobile,
                            Email = xy.x.emailAddress,
                            extensionNo = xy.x.ExtnNo,
                            designation = xy.x.Post,
                            Department = xy.x.DeptDFCCIL,
                            PgOrder = y != null ? y.PGOrder : null
                        }
                    )
                    .OrderByDescending(x => x.PgOrder)
                    .AsNoTracking()
                    .ToListAsync();

                responseModel.Message = "Directory fetched successfully.";
                responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                responseModel.Data = employeeMaster;
                responseModel.TotalRecords = employeeMaster.Count();

                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = $"An error occurred: {ex.Message}";
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }
        public async Task<ResponseModel> UploadAboutUsAsync(UploadAboutUsDto dto)
        {
            var response = new ResponseModel();

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.EmployeeCode))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "Employee code is required.";
                    return response;
                }

                var employee = await context.MstEmployeeMasters
                    .FirstOrDefaultAsync(e => e.EmployeeCode == dto.EmployeeCode && e.Status == 0);

                if (employee == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Employee not found or inactive.";
                    return response;
                }


                if (dto.PhotoFile != null && dto.PhotoFile.Length > 0)
                {
                    if (dto.PhotoFile.Length > 5 * 1024 * 1024)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = "File size should not exceed 5MB.";
                        return response;
                    }



                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(dto.PhotoFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = "Only JPG, JPEG, PNG, or GIF files are allowed.";
                        return response;
                    }
                    var saveFolder = ((configuration["DeploymentModes"] ?? string.Empty) == "DFCCIL_UAT")
      ? (configuration["EmployeeImagePathUat"] ?? string.Empty)
      : (configuration["EmployeeImagePathProd"] ?? string.Empty);
                    string fileName = $"{dto.EmployeeCode}_{DateTime.Now:yyyyMMdd_HHmmssfff}{fileExtension}";
                    string savePath = Path.Combine(saveFolder, fileName);


                    if (!string.IsNullOrEmpty(employee.Photo))
                    {
                        string oldFilePath = Path.Combine(saveFolder, employee.Photo);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }


                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await dto.PhotoFile.CopyToAsync(stream);
                    }


                    employee.Photo = fileName;


                }

                employee.AboutUs = dto.AboutUs;
                employee.Modify_Date=DateTime.Now;
                await context.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "AboutUs and photo updated successfully.";
                response.Data = new { employee.EmployeeCode, employee.Photo, employee.AboutUs, employee.Modify_Date };

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }

        //public async Task<ResponseModel> GetEmployeeProfile(string EmpCode)
        //{
        //    try
        //    {
        //        var cacheKey = $"GetEmployeeProfile_{EmpCode ?? "X"}";
        //        var versionCacheKey = $"{cacheKey}_version";


        //        var currentDataVersion = await _dbChangeService.GetDataVersionAsync();


        //        var cachedVersion = await _cacheService.GetAsync<long?>(versionCacheKey);

        //        if (cachedVersion == null || cachedVersion != currentDataVersion)
        //        {

        //            await _cacheService.RemoveAsync(cacheKey);


        //            await _cacheService.SetAsync(versionCacheKey, currentDataVersion, TimeSpan.FromHours(2));

        //            _logger.LogInformation("Data version changed, cache invalidated");
        //        }

        //        var responseModel = await _cacheService.GetOrSetAsync(
        //            cacheKey,
        //            async () => await GetEmployeeProfileData(EmpCode),
        //            TimeSpan.FromMinutes(30),
        //            TimeSpan.FromHours(2)
        //        );
        //       // responseModel.Message=$"Data:{currentDataVersion},Cache:{cachedVersion}";


        //        return responseModel;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in GetEmployeeProfile");
        //        return new ResponseModel
        //        {
        //            Message = $"An error occurred: {ex.Message}",
        //            StatusCode = HttpStatusCode.InternalServerError
        //        };
        //    }
        //}
        public async Task<ResponseModel> GetEmployeeProfile(string EmpCode)
        {
            try
            {
                var cacheKey = $"GetEmployeeProfile_{empCode ?? "X"}";
                var versionCacheKey = $"{cacheKey}_version";
                var globalVersionKey = "EmployeeProfile_DataVersion";


                var cachedDbVersion = await _cacheService.GetAsync<long?>(globalVersionKey);
                if (cachedDbVersion == null)
                {
                    cachedDbVersion = await _dbChangeService.GetDataVersionAsync();
                    await _cacheService.SetAsync(globalVersionKey, cachedDbVersion, TimeSpan.FromMinutes(5));
                    _logger.LogInformation("Fetched fresh DB version {Version} and cached it", cachedDbVersion);
                }


                var cachedProfileVersion = await _cacheService.GetAsync<long?>(versionCacheKey);


                if (cachedProfileVersion == null || cachedProfileVersion != cachedDbVersion)
                {
                    await _cacheService.RemoveAsync(cacheKey);
                    await _cacheService.SetAsync(versionCacheKey, cachedDbVersion, TimeSpan.FromHours(2));

                    _logger.LogInformation(
                        "EmployeeProfile cache invalidated for EmpCode {EmpCode}. DBVersion={DbVersion}, CachedProfileVersion={ProfileVersion}",
                        empCode, cachedDbVersion, cachedProfileVersion
                    );
                }


                var responseModel = await _cacheService.GetOrSetAsync(
                    cacheKey,
                    async () => await GetEmployeeProfileData(empCode),
                    TimeSpan.FromHours(2),
                    TimeSpan.FromHours(2)
                );



                return responseModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetEmployeeProfile for EmpCode {EmpCode}", empCode);
                return new ResponseModel
                {
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ResponseModel> GetEmployeeProfileData(string empCode)
        {
            try
            {
                var count = 1;
                using var connection = dapper.GetConnection();

                using var multi = await connection.QueryMultipleAsync(
                    "[dbo].[GetEmployeeOptimise]",
                    new { EmployeeCode = empCode },
                    commandType: CommandType.StoredProcedure
                );

                var employees = await multi.ReadAsync<EmployeeProfileDto>();
                var units = await multi.ReadAsync<UnitDto>();

                var employee = employees.ToList();

                var Grade = context.mstPositionGreades.Select(x => new
                {
                    x.PositionGrade,
                    x.PGOrder
                }).OrderByDescending(x => x.PGOrder).ToList();

                var result = new
                {
                    employee = employee,
                    units = units.ToList(),
                    PositionGrades = Grade
                };
                count=count+1;
                return new ResponseModel()
                {
                    StatusCode=HttpStatusCode.OK,
                    Data=result,
                    Message=$"Employee Details Fetched Successfully{count}."


                };
            }
            catch (Exception ex)
            {

                return new ResponseModel()
                {
                    StatusCode=HttpStatusCode.InternalServerError,
                    Message=ex.Message,

                };
            }
        }
        public async Task<ResponseModel> GetAllMastersAsync()
        {
            var response = new ResponseModel();

            try
            {
                var EmployeeType = await context.MstEmployeeMasters.Where(x => x.Status==0 && x.TOemploy!=null).Select(x => x.TOemploy).Distinct().ToListAsync();

                var positionGrades = await context.mstPositionGreades
                    .OrderByDescending(p => p.PGOrder)
                    .Select(p => new { p.PositionGrade, p.PGOrder })
                    .ToListAsync();


                var posts = await context.MstPosts
                    .Where(p => p.Status == 0)
                    .OrderBy(p => p.Post)
                    .Select(p => new { p.Post, p.Description })
                    .ToListAsync();


                var departments = await context.MstDepartments
                    .Where(d => d.Status == 0)
                    .OrderBy(d => d.Department)
                    .Select(d => new { d.Department, d.Departmentid })
                    .ToListAsync();


                var contractors = await context.MstContractMasters
                    .Where(c => c.status == true)
                    .OrderBy(c => c.Contractor)
                    .Select(c => new { c.Contractor, c.PkContractid })
                    .ToListAsync();

                var mstUnit = await context.UnitNameDetails
                   .OrderBy(c => c.SequenceID)
                   .Select(c => new { c.Id, c.Name, c.SequenceID, c.Abbrivation })
                   .ToListAsync();

                var masterData = new
                {
                    Unit = mstUnit,
                    PositionGrades = positionGrades,
                    Posts = posts,
                    Departments = departments,
                    Contractors = contractors,
                    EmployeeTypes = EmployeeType
                };

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Master data fetched successfully.";
                response.Data = masterData;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                response.Data = null;
            }

            return response;
        }


        public async Task<ResponseModel> GetSelectedEmployeeColumnsAsync(string columnNamesCsv, string? employeeCode = null)
        {
            var response = new ResponseModel();
            var conncetion = dapper.GetDbconnection();
            try
            {
                if (string.IsNullOrWhiteSpace(columnNamesCsv))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "No columns specified.";
                    return response;
                }

                var selectedColumns = columnNamesCsv.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(c => c.Trim())
                                                    .ToHashSet(StringComparer.OrdinalIgnoreCase);


                var allColumns = (await conncetion.QueryAsync<string>(
                    @"SELECT COLUMN_NAME 
                  FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'MstEmployeeMaster'")).ToList();

                var validColumns = selectedColumns.Intersect(allColumns).ToList();
                if (!validColumns.Any())
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "No valid columns found.";
                    return response;
                }

                var columnList = string.Join(",", validColumns);

                var sql = $@"SELECT {columnList}
                         FROM MstEmployeeMaster
                         WHERE Status = 0";

                if (!string.IsNullOrEmpty(employeeCode))
                {
                    sql += " AND EmployeeCode = @empCode";
                }

                var data = await conncetion.QueryAsync(sql, new { empCode = employeeCode });
                var result = data.Select(row => (IDictionary<string, object>)row).ToList();

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Employee data fetched successfully.";
                response.Data = result;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseModel> GetEmployeeMasterColumnsAsync()
        {
            var response = new ResponseModel();
            try
            {
                var allColumns = new List<string>();
                using (var conn = context.Database.GetDbConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT COLUMN_NAME 
                                    FROM INFORMATION_SCHEMA.COLUMNS 
                                    WHERE TABLE_NAME = 'MstEmployeeMaster'";
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                allColumns.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Column names fetched successfully.";
                response.Data = allColumns;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }




        #region HolidayCalender

        public async Task<ResponseModel> GetAllHolidays(int? unitId = null, string? holidayType = null, string? unitName = null)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var query = context.MasterHolidayCalendars
                    .Where(h => h.IsActive == true);

                if (unitId.HasValue)
                {
                    query = query.Where(h => h.UnitId == unitId.Value);
                }

                if (!string.IsNullOrWhiteSpace(holidayType))
                {
                    query = query.Where(h => h.HolidayType == holidayType);
                }

                if (!string.IsNullOrWhiteSpace(unitName))
                {
                    query = query.Where(h => h.UnitName != null && h.UnitName.Contains(unitName));
                }

                var result = await query.Select(h => new HolidayCalendarDto
                {
                    HolidayId = h.HolidayId,
                    SerialNumber = h.SerialNumber,
                    HolidayDate = h.HolidayDate,
                    HolidayDescription = h.HolidayDescription,
                    HolidayType = h.HolidayType,
                    DayOfWeek = h.DayOfWeek,
                    UnitId = h.UnitId,
                    UnitName = h.UnitName,
                    IsActive = h.IsActive.Value,
                    CreatedBy = h.CreatedBy,
                    CreatedDate = h.CreatedDate,
                    UpdatedBy = h.UpdatedBy,
                    UpdatedDate = h.UpdatedDate
                }).OrderBy(h => h.HolidayDate).ToListAsync();

                responseModel.Message = "Data fetched successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = result;
                responseModel.TotalRecords = result.Count;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = "Internal Server Error";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }

        public async Task<ResponseModel> GetHolidaysByDateRange(DateTime? fromDate = null, DateTime? toDate = null, int? unitId = null)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var query = context.MasterHolidayCalendars
                    .Where(h => h.IsActive == true);

                if (fromDate.HasValue)
                {
                    query = query.Where(h => h.HolidayDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(h => h.HolidayDate <= toDate.Value);
                }

                if (unitId.HasValue)
                {
                    query = query.Where(h => h.UnitId == unitId.Value);
                }

                var result = await query.Select(h => new HolidayCalendarDto
                {
                    HolidayId = h.HolidayId,
                    SerialNumber = h.SerialNumber,
                    HolidayDate = h.HolidayDate,
                    HolidayDescription = h.HolidayDescription,
                    HolidayType = h.HolidayType,
                    DayOfWeek = h.DayOfWeek,
                    UnitId = h.UnitId,
                    UnitName = h.UnitName,
                    IsActive = h.IsActive.Value,
                    CreatedBy = h.CreatedBy,
                    CreatedDate = h.CreatedDate,
                    UpdatedBy = h.UpdatedBy,
                    UpdatedDate = h.UpdatedDate
                }).OrderBy(h => h.HolidayDate).ToListAsync();

                responseModel.Message = "Data fetched successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = result;
                responseModel.TotalRecords = result.Count;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = "Internal Server Error";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }

        public async Task<ResponseModel> CreateHoliday(CreateHolidayCalendarDto createHolidayDto,string loginUserId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {

                var existingHoliday = await context.MasterHolidayCalendars
                    .FirstOrDefaultAsync(h => h.HolidayDate == createHolidayDto.HolidayDate
                                           && h.UnitId == createHolidayDto.UnitId
                                           && h.IsActive == true);

                if (existingHoliday != null)
                {
                    responseModel.Message = "Holiday already exists for this date and unit";
                    responseModel.StatusCode = HttpStatusCode.Conflict;
                    return responseModel;
                }

                var holiday = new MasterHolidayCalendar
                {
                    HolidayDate = createHolidayDto.HolidayDate,
                    HolidayDescription = createHolidayDto.HolidayDescription,
                    HolidayType = createHolidayDto.HolidayType,
                    DayOfWeek = createHolidayDto.DayOfWeek,
                    UnitId = createHolidayDto.UnitId,
                    UnitName = createHolidayDto.UnitName,
                    IsActive = true,
                    CreatedBy = loginUserId,
                    CreatedDate = DateTime.Now
                };

                context.MasterHolidayCalendars.Add(holiday);
                await context.SaveChangesAsync();

                responseModel.Message = "Holiday created successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = holiday.HolidayId;
                responseModel.TotalRecords = 1;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = "Internal Server Error";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }

        public async Task<ResponseModel> UpdateHoliday(UpdateHolidayCalendarDto updateHolidayDto, string loginUserId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var holiday = await context.MasterHolidayCalendars
                    .FirstOrDefaultAsync(h => h.HolidayId == updateHolidayDto.HolidayId && h.IsActive == true);

                if (holiday == null)
                {
                    responseModel.Message = "Holiday not found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }

                holiday.HolidayDate = updateHolidayDto.HolidayDate;
                holiday.HolidayDescription = updateHolidayDto.HolidayDescription;
                holiday.HolidayType = updateHolidayDto.HolidayType;
                holiday.DayOfWeek = updateHolidayDto.DayOfWeek;
                holiday.UnitId = updateHolidayDto.UnitId;
                holiday.UnitName = updateHolidayDto.UnitName;
                holiday.UpdatedBy = loginUserId;
                holiday.UpdatedDate = DateTime.Now;

                context.MasterHolidayCalendars.Update(holiday);
                await context.SaveChangesAsync();

                responseModel.Message = "Holiday updated successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = holiday.HolidayId;
                responseModel.TotalRecords = 1;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = "Internal Server Error";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }

        public async Task<ResponseModel> DeleteHoliday(int holidayId, string? loginUserId = null)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var holiday = await context.MasterHolidayCalendars
                    .FirstOrDefaultAsync(h => h.HolidayId == holidayId && h.IsActive == true);

                if (holiday == null)
                {
                    responseModel.Message = "Holiday not found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }


                holiday.IsActive = false;
                holiday.UpdatedBy = loginUserId;
                holiday.UpdatedDate = DateTime.Now;

                context.MasterHolidayCalendars.Update(holiday);
                await context.SaveChangesAsync();

                responseModel.Message = "Holiday deleted successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = holidayId;
                responseModel.TotalRecords = 1;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = "Internal Server Error";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }

        public async Task<ResponseModel> BulkCreateHolidays(List<CreateHolidayCalendarDto> holidays,string loginUserId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var holidayEntities = new List<MasterHolidayCalendar>();

                foreach (var holidayDto in holidays)
                {

                    var existingHoliday = await context.MasterHolidayCalendars
                        .FirstOrDefaultAsync(h => h.HolidayDate == holidayDto.HolidayDate
                                               && h.UnitId == holidayDto.UnitId
                                               && h.IsActive == true);

                    if (existingHoliday == null)
                    {
                        var holiday = new MasterHolidayCalendar
                        {

                            HolidayDate = holidayDto.HolidayDate,
                            HolidayDescription = holidayDto.HolidayDescription,
                            HolidayType = holidayDto.HolidayType,
                            DayOfWeek = holidayDto.DayOfWeek,
                            UnitId = holidayDto.UnitId,
                            UnitName = holidayDto.UnitName,
                            IsActive = true,
                            CreatedBy = loginUserId,
                            CreatedDate = DateTime.Now
                        };
                        holidayEntities.Add(holiday);
                    }
                }

                if (holidayEntities.Any())
                {
                    context.MasterHolidayCalendars.AddRange(holidayEntities);
                    await context.SaveChangesAsync();
                }

                responseModel.Message = $"{holidayEntities.Count} holidays created successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = holidayEntities.Select(h => h.HolidayId).ToList();
                responseModel.TotalRecords = holidayEntities.Count;
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = "Internal Server Error";
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Data = ex.Message;
                return responseModel;
            }
        }

        public async Task<ResponseModel> UploadHolidaysFromExcel(IFormFile file, int unitId, string unitName, string loginUserId)
        {
            var responseModel = new ResponseModel();
            try
            {
                if (file == null || file.Length == 0)
                    return new ResponseModel { Message = "Please select a valid Excel file", StatusCode = HttpStatusCode.BadRequest };

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                    return new ResponseModel { Message = "Only Excel files (.xlsx, .xls) are allowed", StatusCode = HttpStatusCode.BadRequest };

                if (!await context.UnitNameDetails.AnyAsync(u => u.Id == unitId && u.IsActive))
                    return new ResponseModel { Message = "Invalid Unit ID", StatusCode = HttpStatusCode.BadRequest };

                var holidaysToCreate = new List<CreateHolidayCalendarDto>();
                var validationErrors = new List<string>();
                int processedCount = 0, duplicateCount = 0;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return new ResponseModel { Message = "No worksheet found in the Excel file", StatusCode = HttpStatusCode.BadRequest };

               
                int headerRow = 0;
                var usedRange = worksheet.RangeUsed();
                if (usedRange != null)
                {
                    for (int row = 1; row <= Math.Min(10, usedRange.LastRow().RowNumber()); row++)
                    {
                        var colDate = worksheet.Cell(row, 3).GetValue<string>();
                        var colDesc = worksheet.Cell(row, 4).GetValue<string>();

                        if (!string.IsNullOrEmpty(colDate) && colDate.Contains("Date") &&
                            !string.IsNullOrEmpty(colDesc) && colDesc.Contains("Holiday Description"))
                        {
                            headerRow = row;
                            break;
                        }
                    }
                }

                if (headerRow == 0)
                    return new ResponseModel
                    {
                        Message = "Invalid Excel format. Expected headers: Date, Holiday Description, Holiday Type, Day",
                        StatusCode = HttpStatusCode.BadRequest
                    };

                int totalRows = usedRange?.LastRow()?.RowNumber() ?? 0;

                for (int row = headerRow + 1; row <= totalRows; row++)
                {
                    try
                    {
                        var dateCell = worksheet.Cell(row, 3);
                        var descriptionCell = worksheet.Cell(row, 4);
                        var typeCell = worksheet.Cell(row, 5);
                        var dayCell = worksheet.Cell(row, 6);

                        if (dateCell.IsEmpty())
                            continue;

                        
                        if (!DateTime.TryParse(dateCell.GetValue<string>(), out DateTime holidayDate))
                        {
                            validationErrors.Add($"Row {row}: Invalid Date format");
                            continue;
                        }

                        var description = descriptionCell.GetValue<string>()?.Trim();
                        var holidayType = typeCell.GetValue<string>()?.Trim();
                        var dayOfWeek = string.IsNullOrEmpty(dayCell.GetValue<string>())
                            ? holidayDate.DayOfWeek.ToString()
                            : dayCell.GetValue<string>().Trim();

                        if (string.IsNullOrEmpty(description))
                        {
                            validationErrors.Add($"Row {row}: Holiday Description is required");
                            continue;
                        }

                        if (holidayType != "GH" && holidayType != "RH")
                        {
                            validationErrors.Add($"Row {row}: Holiday Type must be 'GH' or 'RH'");
                            continue;
                        }

                        if (await context.MasterHolidayCalendars.AnyAsync(h =>
                           h.UnitId == unitId &&
                           h.IsActive.Value &&
                           h.HolidayDate >= holidayDate.Date &&
                           h.HolidayDate < holidayDate.Date.AddDays(1)))
                        {
                            duplicateCount++;
                            validationErrors.Add($"Row {row}: Holiday already exists for {holidayDate:yyyy-MM-dd}");
                            continue;
                        }

                       
                        if (holidaysToCreate.Any(h => h.HolidayDate.Date == holidayDate.Date))
                        {
                            validationErrors.Add($"Row {row}: Duplicate date {holidayDate:yyyy-MM-dd} in Excel file");
                            continue;
                        }

                        holidaysToCreate.Add(new CreateHolidayCalendarDto
                        {
                            HolidayDate = holidayDate,
                            HolidayDescription = description,
                            HolidayType = holidayType,
                            DayOfWeek = dayOfWeek,
                            UnitId = unitId,
                            UnitName = unitName
                           
                        });

                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        validationErrors.Add($"Row {row}: Error processing row - {ex.Message}");
                    }
                }

                int createdCount = 0;
                if (holidaysToCreate.Any())
                {
                    var bulkCreateResult = await BulkCreateHolidays(holidaysToCreate,loginUserId);
                    if (bulkCreateResult.StatusCode == HttpStatusCode.OK)
                        createdCount = holidaysToCreate.Count;
                    else
                        return new ResponseModel
                        {
                            Message = "Error creating holidays: " + bulkCreateResult.Message,
                            StatusCode = HttpStatusCode.InternalServerError,
                            Data = bulkCreateResult.Data
                        };
                }

                var result = new
                {
                    TotalRecordsProcessed = processedCount,
                    HolidaysCreated = createdCount,
                    DuplicatesFound = duplicateCount,
                    ValidationErrors = validationErrors,
                    Success = createdCount > 0
                };

                return new ResponseModel
                {
                    Message = $"Excel upload completed. {createdCount} holidays created successfully",
                    StatusCode = HttpStatusCode.OK,
                    Data = result,
                    TotalRecords = createdCount
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Message = "Error processing Excel file",
                    StatusCode = HttpStatusCode.InternalServerError,
                    Data = ex.Message
                };
            }
        }



        #endregion

    }


}

