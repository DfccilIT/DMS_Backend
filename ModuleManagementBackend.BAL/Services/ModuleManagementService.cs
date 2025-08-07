using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using System.Data;
using System.Net;
using System.Runtime.InteropServices;

namespace ModuleManagementBackend.BAL.Services
{
    public class ModuleManagementService : IModuleManagementService
    {
        private readonly SAPTOKENContext context;
        private readonly IDapperService dapper;
        private readonly IHttpContextAccessor httpContext;

        public ModuleManagementService(SAPTOKENContext context, IDapperService dapper, IHttpContextAccessor httpContext)
        {
            this.context=context;
            this.dapper=dapper;
            this.httpContext=httpContext;
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

                if (!isApproved)
                {
                    response.Message = "IsApproved is mandatory.";
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
                    mstRecord.ReportingOfficer = editRecord.ReportingOfficer;
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
                var query = context.EditEmployeeDetails.Where(e => e.status == 99 && e.TableName == "R");
                if (!string.IsNullOrWhiteSpace(employeeCode))
                {
                    query = query.Where(e => e.EmployeeCode == employeeCode);
                }

                if (!string.IsNullOrWhiteSpace(location))
                {
                    query = query.Where(e => e.Location != null && e.Location.Contains(location));
                }

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    query = query.Where(e => e.UserName != null && e.UserName.Contains(userName));
                }

                var result = await query.ToListAsync();


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
            : $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/Images/Employees/{x.fkEmployeeMasterAuto.Photo}",
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
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;
            try
            {
                var records = await context.tblEmployeeOfTheMonths
                    .Where(x => x.status == 0 && x.mnth==currentMonth && x.yr==currentYear)
                    .Select(x => new
                    {
                        EMPCode = x.fkEmployeeMasterAuto.EmployeeCode,
                        EmployeeName = x.fkEmployeeMasterAuto.UserName,
                        Designation = x.fkEmployeeMasterAuto.GenericDesignation,
                        Department = x.fkEmployeeMasterAuto.DeptDFCCIL,
                        PhotoUrl = x.photo != null
            ? $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/EmployeeOfTheMonth/{x.photo}"
            : $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/Images/Employees/{x.fkEmployeeMasterAuto.Photo}",
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
                        existingRecord.photo = await UploadPhotoIfAvailable(dto.photo);
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
                    response.Data = newEntry;
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
                var notice = new tblNoticeBoard
                {
                    msg = dto.Msg,
                    doc = dto.Doc,
                    status = dto.Status ?? 0,
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

                notice.msg = dto.Msg;
                notice.doc = dto.Doc;
                notice.status = dto.Status;
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

                if (string.IsNullOrWhiteSpace(empCode))
                {
                    response.Message = "EmployeeCode is required.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

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
                    if (dto.pkDependentId > 0)
                    {
                        var existing = await context.MstEmployeeDependents.FindAsync(dto.pkDependentId);
                        if (existing != null)
                        {
                            existing.Relation = dto.Relation ?? existing.Relation;
                            existing.DName = dto.DName ?? existing.DName;
                            existing.Gender = dto.Gender ?? existing.Gender;
                            if (dto.Age > 0) existing.Age = dto.Age;
                            existing.status = 99;
                            existing.updatedBy = loginUserEmpCode;
                            existing.updatedDate = DateTime.Now;

                            context.SaveChanges();
                        }
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
                            createdBy= loginUserEmpCode,
                            createdDate = DateTime.Now,
                        };

                        await context.MstEmployeeDependents.AddAsync(newDependent);
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

                if (!isApproved)
                {
                    response.Message = "IsApproved is mandatory.";
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
                .FirstOrDefaultAsync(e => e.fkEmployeeMasterAutoId == employee.EmployeeMasterAutoId && e.status == 99 );

                if (editRecord == null)
                {
                    response.Message = "No pending edit request found for this employee.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                if (isApproved)
                {
                    editRecord.updatedDate=DateTime.Now.Date;
                    editRecord.updatedBy =LoginUserEmpCode;
                    editRecord.status = 0;
                    context.SaveChanges();


                    response.Message = "Approved successfully.";
                    response.StatusCode = HttpStatusCode.OK;
                    response.Data = new { success = true, ApprovedSuccessfully = true };
                    return response;
                }
                else
                {
                    editRecord.updatedDate=DateTime.Now.Date;
                    editRecord.updatedBy =LoginUserEmpCode;
                    editRecord.remarks = remarks;
                    editRecord.status = 9;
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

        public async Task<ResponseModel> GetAllDependentsRequestByEmpCodeAsync()
        {
            var response = new ResponseModel();

            try
            {
                var dependents = await context.MstEmployeeDependents
                    .Where(d => d.status == 99)
                    .Select(d => new
                    {
                        d.pkDependentId,
                        d.DName,
                        d.Relation,
                        d.Gender,
                        d.Age,
                        d.status
                    })
                    .ToListAsync();

                response.Message = "Dependents Requests fetched successfully.";
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
                    .Where(d => d.fkEmployeeMasterAutoId == fkEmployeeMasterAutoId && d.status==99)
                    .Select(d => new
                    {
                        d.pkDependentId,
                        d.DName,
                        d.Relation,
                        d.Gender,
                        d.Age,
                        d.status
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

        }
    }
}
