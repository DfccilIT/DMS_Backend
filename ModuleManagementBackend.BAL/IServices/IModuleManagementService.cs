using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.EditEmployeeDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IModuleManagementService
    {

        Task<ResponseModel> ProcessEditEmployeeRequest(AprooveEmployeeReportDto request);
        Task<ResponseModel> GetAllReportingOfficerRequest(string? employeeCode = null, string? location = null, string? userName = null);
        Task<ResponseModel> ProcessEditReportingOfficerRequest(AprooveEmployeeReportDto request);
        Task<ResponseModel> GetDfccilDirectory(string? EmpCode = null);
    }
}
