using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using System.Data;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ModuleManagementBackend.BAL.Services
{
    public class ModuleManagementService : IModuleManagementService
    {
        private readonly SAPTOKENContext context;
        private readonly IDapperService dapper;
        private readonly IHttpContextAccessor httpContext;
        private readonly IConfiguration configuration;
        private readonly string baseUrl;
        public ModuleManagementService(SAPTOKENContext context, IDapperService dapper, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            this.context=context;
            this.dapper=dapper;
            this.httpContext=httpContext;
            this.configuration=configuration;
            this.baseUrl = ((configuration["DeploymentModes"] ?? string.Empty) == "DFCCIL_UAT")
    ? (configuration["BasePathUat"] ?? string.Empty)
    : (configuration["BasePathProd"] ?? string.Empty);
        }

        public async Task<ResponseModel> GetAllEditEmployeeRequests(string? employeeCode = null, string? location = null, string? userName = null)
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
                    SubDepartment = x.mm.SubDeptDF,
                    x.mm.DOB,
                    DateOfAnniversary = x.mm.AnniversaryDate,
                    DateOfJoining = x.mm.DOJDFCCIL,
                    x.mm.Location,
                    SubArea = x.mm.PersonnelSubArea,
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
                    x.ee.SubDepartment,
                    x.ee.DOB,
                    x.ee.DateOfAnniversary,
                    x.ee.DateOfJoining,
                    x.ee.Location,
                    x.ee.SubArea,
                    x.ee.Mobile,
                    x.ee.Email,
                    x.ee.PersonalEmailId,
                    x.ee.Toemploy,
                    x.ee.AboutUs,
                    x.ee.Photo,
                    x.ee.ReportingOfficer,
                    x.ee.ExtensionNo,
                },
            }).ToListAsync();


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
                    mstRecord.SubDeptDF = editRecord.SubDepartment;
                    mstRecord.DOB = editRecord.DOB;
                    mstRecord.AnniversaryDate = editRecord.DateOfAnniversary;
                    mstRecord.DOJDFCCIL = editRecord.DateOfJoining;
                    mstRecord.Location = editRecord.Location;
                    mstRecord.PersonnelSubArea = editRecord.SubArea;
                    mstRecord.Mobile = editRecord.Mobile;
                    mstRecord.emailAddress = editRecord.Email;
                    mstRecord.PersonalEmailAddress = editRecord.PersonalEmailId;
                    mstRecord.TOemploy = editRecord.Toemploy;
                    mstRecord.AboutUs = editRecord.AboutUs;
                    mstRecord.Photo = editRecord.Photo;
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
                        SubDepartment = x.mm.SubDeptDF,
                        x.mm.DOB,
                        DateOfAnniversary = x.mm.AnniversaryDate,
                        DateOfJoining = x.mm.DOJDFCCIL,
                        x.mm.Location,
                        SubArea = x.mm.PersonnelSubArea,
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
                        x.ee.SubDepartment,
                        x.ee.DOB,
                        x.ee.DateOfAnniversary,
                        x.ee.DateOfJoining,
                        x.ee.Location,
                        x.ee.SubArea,
                        x.ee.Mobile,
                        x.ee.Email,
                        x.ee.PersonalEmailId,
                        x.ee.Toemploy,
                        x.ee.AboutUs,
                        x.ee.Photo,
                        x.ee.ReportingOfficer,
                        x.ee.ExtensionNo,
                    },
                }).ToListAsync();



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
        public async Task<ResponseModel> GetDfccilDirectory(string? EmpCode = null)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {

                var Employeemaster = await context.MstEmployeeMasters.Where(x => (x.EmployeeCode==EmpCode ||EmpCode==null) && x.Status==0).
                      Select(x => new
                      {
                          EmpCode = x.EmployeeCode,
                          name = x.UserName,
                          unit = x.Location,
                          personalMobile = x.PersonalMobile,
                          Email = x.emailAddress,
                          extensionNo = x.ExtnNo,
                          designation = x.GenericDesignation,
                          Department = x.DeptDFCCIL

                      }).ToListAsync();

                responseModel.Message = "Directory fetched successfully.";
                responseModel.StatusCode= System.Net.HttpStatusCode.OK;
                responseModel.Data = Employeemaster;
                responseModel.TotalRecords=Employeemaster.Count();

                return responseModel;
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


                if (!string.IsNullOrEmpty(updateDto.PersonalMobile)) employee.PersonalMobile = updateDto.PersonalMobile;
                if (!string.IsNullOrEmpty(updateDto.ExtnNo)) employee.ExtnNo = updateDto.ExtnNo;

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
                    .Where(x => x.status == 0)
                    .OrderByDescending(x => x.yr)
                    .ThenByDescending(x => x.mnth)
                    .Select(x => new
                    {
                        EMPCode = x.fkEmployeeMasterAuto.EmployeeCode,
                        EmployeeName = x.fkEmployeeMasterAuto.UserName,
                        Designation = x.fkEmployeeMasterAuto.GenericDesignation,
                        Department = x.fkEmployeeMasterAuto.DeptDFCCIL,
                        PhotoUrl = x.photo != null
            ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/EmployeeOfTheMonth/{x.photo}"
            : $"{baseUrl}/Images/Employees/{x.fkEmployeeMasterAuto.Photo}",
                        Month = x.mnth,
                        Year = x.yr,
                        x.createDate,
                        x.createBy
                    }).Take(5)
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
            //var currentDate = DateTime.Now;
            //var currentMonth = currentDate.Month;
            //var currentYear = currentDate.Year;
            try
            {
                var records = await context.tblEmployeeOfTheMonths
                    .Where(x => x.status == 0).
                    OrderByDescending(x => x.yr).
                    ThenByDescending(x => x.mnth)
                    .Select(x => new
                    {
                        EMPCode = x.fkEmployeeMasterAuto.EmployeeCode,
                        EmployeeName = x.fkEmployeeMasterAuto.UserName,
                        Designation = x.fkEmployeeMasterAuto.GenericDesignation,
                        Department = x.fkEmployeeMasterAuto.DeptDFCCIL,
                        PhotoUrl = x.photo != null
            ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/EmployeeOfTheMonth/{x.photo}"
            : $"{baseUrl}/Images/Employees/{x.fkEmployeeMasterAuto.Photo}",
                        Month = x.mnth,
                        Year = x.yr,
                        x.createDate,
                        x.createBy
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
                       var fileName= await UploadPhotoIfAvailable(dto.photo);
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
                var employee = context.MstEmployeeMasters
                    .FirstOrDefault(e => e.EmployeeCode == empCode && e.Status == 0);

                if (employee == null)
                {
                    response.Message = "Employee not found or inactive.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                long fkEmployeeMasterAutoId = employee.EmployeeMasterAutoId;

                foreach (var dto in dependents)
                {

                    var existing = await context.MstEmployeeDependents.FirstOrDefaultAsync(x => x.fkEmployeeMasterAutoId==fkEmployeeMasterAutoId && x.status==99);
                    if (existing != null)
                    {
                        response.Message = "There is already a pending request for this employee.";
                        response.StatusCode = HttpStatusCode.Conflict;
                        return response;
                        //existing.Relation = dto.Relation ?? existing.Relation;
                        //existing.DName = dto.DName ?? existing.DName;
                        //existing.Gender = dto.Gender ?? existing.Gender;
                        //if (dto.Age > 0) existing.Age = dto.Age;

                        //existing.status = 99;
                        //existing.updatedBy = loginUserEmpCode;
                        //existing.updatedDate = DateTime.Now.Date;
                        //context.MstEmployeeDependents.Update(existing);
                        //if (dto.DocumentFiles != null && dto.DocumentFiles.Any())
                        //{
                        //    await UploadDependentDOCIfAvailable(dto.DocumentFiles, existing.pkDependentId, loginUserEmpCode,"UPDATE");
                        //}
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(dto.Relation) ||
                        string.IsNullOrWhiteSpace(dto.DName) ||
                        string.IsNullOrWhiteSpace(dto.Gender) ||
                        dto.Age <= 0)
                        {
                            response.Message = $"Validation failed for dependent: {dto?.DName}";
                            response.StatusCode = HttpStatusCode.BadRequest;
                            return response;
                        }

                        bool exists = context.MstEmployeeDependents.Any(x =>
                            x.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId &&
                            x.DName.ToLower() == dto.DName.Trim().ToLower() &&
                            x.Relation.ToLower() == dto.Relation.Trim().ToLower());

                        if (exists)
                        {
                            response.Message = $"Dependent '{dto.DName}' with relation '{dto.Relation}' already exists.";
                            response.StatusCode = HttpStatusCode.Conflict;
                            return response;
                        }

                        var newDependent = new MstEmployeeDependent
                        {
                            fkEmployeeMasterAutoId = fkEmployeeMasterAutoId,
                            Relation = dto.Relation.Trim(),
                            DName = dto.DName.Trim(),
                            Gender = dto.Gender.Trim(),
                            Age = dto.Age,
                            status = 99,
                            createdBy = loginUserEmpCode,
                            createdDate = DateTime.Now.Date
                        };

                        await context.MstEmployeeDependents.AddAsync(newDependent);
                        await context.SaveChangesAsync();


                        if (dto.DocumentFiles != null && dto.DocumentFiles.Any())
                        {
                            await UploadDependentDOCIfAvailable(dto.DocumentFiles, newDependent.pkDependentId, loginUserEmpCode);
                        }
                    }

                }


                await context.SaveChangesAsync();

                response.Message = "Dependents processed successfully.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = "An error occurred: " + ex.Message;
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
                var groupedDependents = await context.MstEmployeeDependents
                    .Where(dep => dep.status == 99)
                    .Join(context.MstEmployeeMasters,
                        dep => dep.fkEmployeeMasterAutoId,
                        emp => emp.EmployeeMasterAutoId,
                        (dep, emp) => new
                        {
                            emp.EmployeeCode,
                            dep.DName,
                            dep.Relation,
                            dep.Gender,
                            dep.Age,
                            dep.status,
                            dep.EmployeeDependentDocuments
                        })
                    .GroupBy(x => x.EmployeeCode)
                    .Select(g => new
                    {
                        EmployeeCode = g.Key,
                        Dependents = g.Select(d => new
                        {
                            d.DName,
                            d.Relation,
                            d.Gender,
                            d.Age,
                            d.status,
                            DocumentList = d.EmployeeDependentDocuments.Select(doc => new
                            {
                                doc.DocumentId,
                                filePath = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/DependentDocuments/{doc.DocumentName}",
                                doc.DocumentName,
                                doc.DocumentType,
                                doc.Remarks
                            }).ToList()
                        }).ToList()
                    })
                    .ToListAsync();

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

                var dependents = await context.MstEmployeeDependents
                    .Join(context.MstEmployeeMasters,
                    MD => MD.fkEmployeeMasterAutoId,
                    MM => MM.EmployeeMasterAutoId,
                    (MD, MM) => new
                    {
                        MD,
                        MM
                    })
                    .Where(d => d.MD.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId && d.MD.status==99)
                    .Select(d => new
                    {
                        d.MD.pkDependentId,
                        d.MM.EmployeeCode,
                        d.MD.DName,
                        d.MD.Relation,
                        d.MD.Gender,
                        d.MD.Age,
                        d.MD.status,
                        DocumentList = d.MD.EmployeeDependentDocuments.Select(x => new
                        {
                            x.DocumentId,
                            filePath = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/DependentDocuments/{x.DocumentName}",
                            x.DocumentName,
                            x.DocumentType,
                            x.Remarks
                        }).ToList()
                    })
                    .ToListAsync();

                response.Message = "Dependents fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = dependents;
                response.TotalRecords = dependents.Count;
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
                        PersonalMobile = existingEmployee.Mobile,
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


        public ResponseModel GetEmployeeProfile(string EmployeeCode)
        {
            var response = new ResponseModel();
            try
            {
                // Get employee dataset
                var result = GetEmployee(EmployeeCode);

                if (result.Data is not List<Dictionary<string, object>> employeeDetails || employeeDetails.Count == 0)
                {
                    response.Data = null;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Employee profile not found.";
                    return response;
                }

                // Prepare final response
                response.Data = new
                {
                    EmployeeDetails = employeeDetails
                };
                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Employee profile fetched successfully.";
                response.TotalRecords = employeeDetails.Count;
                response.DataLength = employeeDetails.Count;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while fetching employee profile: " + ex.Message;
            }

            return response;
        }


        private ResponseModel GetEmployee(string EmployeeCode)
        {
            var response = new ResponseModel();
            var employeeList = new List<Dictionary<string, object>>();

            try
            {
                using var connection = dapper.GetConnection();
                connection.Open();

                EmployeeCode = string.IsNullOrEmpty(EmployeeCode) ? "X" : EmployeeCode;
                using var command = new SqlCommand("GetEmployeeBySk", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmployeeCode", EmployeeCode);

                using var reader = command.ExecuteReader();
                var columnNames = Enumerable.Range(0, reader.FieldCount)
                                            .Select(reader.GetName)
                                            .ToList();

                while (reader.Read())
                {
                    var row = new Dictionary<string, object>(columnNames.Count);
                    foreach (var col in columnNames)
                    {
                        var value = reader[col];
                        row[col] = value == DBNull.Value ? null : value;
                    }
                    employeeList.Add(row);
                }

                response.Data = employeeList;
                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Employee data fetched successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while fetching employee data: " + ex.Message;
            }

            return response;
        }





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

        public async Task<string> UploadNoticeDOCIfAvailable(IFormFile doc)
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

        public async Task UploadDependentDOCIfAvailable(List<DependtentsDocuments> docs, long fkMstDependentId, string loginUserId, string Flag = "")
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


        /// <summary>
        /// Created By Saurabh Kumar Against Send SMS to Reporting Officer And Employee
        /// </summary>
        /// <param name="username"></param>
        /// <param name="phone"></param>
        /// <param name="msg"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public async Task<ResponseModel> SendSmsAsync(string templateId, string empCode, string userName = "")
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
                string apiUrl = $"https://login.dfccil.com/Dfccil/DFCSMS?username={Uri.EscapeDataString(username)}&Phone={Uri.EscapeDataString(phone)}&msg={encodedMsg}&templatedid={Uri.EscapeDataString(templateId)}";

                using var httpClient = new HttpClient();
                var apiResponse = await httpClient.PostAsync(apiUrl, null);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var resultContent = await apiResponse.Content.ReadAsStringAsync();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "SMS sent successfully.";
                    response.Data = resultContent;
                    response.TotalRecords = 1;
                }
                else
                {
                    var errorContent = await apiResponse.Content.ReadAsStringAsync();
                    response.StatusCode = apiResponse.StatusCode;
                    response.Message = $"Failed to send SMS. Status Code: {apiResponse.StatusCode}";
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


    }
}
