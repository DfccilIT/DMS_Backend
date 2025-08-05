using ModuleManagementBackend.Model.Common;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IAccountService
    {
        Task<ResponseModel> IsValidProgress(string token, string empCode);
        Task<EmployeeDetails> GetEmployeeDetailsAsync(int empCode);
        Task<List<string>> GetUserRolesAsync(int empCode);
    }
}
