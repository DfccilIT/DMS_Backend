using Dapper;
using Microsoft.EntityFrameworkCore;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.DapperServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using ModuleManagementBackend.Model.DTOs.UserDTO;
using System;
using System.Data;
using System.Net;

namespace ModuleManagementBackend.BAL.Services
{
    public class ModuleManagementService : IModuleManagementService
    {
        private readonly SAPTOKENContext context;
        private readonly IDapperService dapper;

        public ModuleManagementService(SAPTOKENContext context, IDapperService dapper)
        {
            this.context=context;
            this.dapper=dapper;
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
                        EmpPhoto = x.fkEmployeeMasterAuto.Photo,
                        Month = x.mnth,
                        Year = x.yr,
                        x.createDate,
                        x.createBy
                    })
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
                        EmpPhoto = x.fkEmployeeMasterAuto.Photo,
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
            ResponseModel response = new ResponseModel();
            var employee = await context.MstEmployeeMasters
                .FirstOrDefaultAsync(e => e.EmployeeCode == dto.EmpCode && e.Status == 0);
            if (employee == null)
            {
                response.Message = "No Employee Found";
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return response;

            }

            try
            {
                string uploadedFileName = null;

                if (dto.photo != null && dto.photo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","EmployeeOftheMonth");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                   
                    uploadedFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.photo.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uploadedFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.photo.CopyToAsync(stream);
                    }
                }

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
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Data = newEntry;

                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return response;
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

        }
    }
}
