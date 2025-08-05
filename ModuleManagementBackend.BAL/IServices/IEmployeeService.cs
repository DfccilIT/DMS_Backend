using ModuleManagementBackend.Model.Common;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IEmployeeService
    {
        Task<EmployeeDetails> GetEmployeeDetails(int empId);
        Task<EmployeeDetails> GetEmployeeDetailsWithEmpCode(int empCode);
        Task<List<EmployeeDetails>> GetOrganizationHierarchy();
    }
}
