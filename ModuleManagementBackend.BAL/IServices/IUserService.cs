using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.UserDTO;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IUserService
    {
        Task<ResponseModel> AddNewRoleWithPermission(DtoAddUserRole model, int EmpCode, string source);
        Task<ResponseModel> AddNewPermission(DtoAddUserPermission model, int EmpCode, string source);
        Task<ResponseModel> GetRoleAndPermissionByEmpCode(int EmpCode);
        Task<ResponseModel> GetEmpRoleList(int unitId, int EmpCode);
        Task<ResponseModel> GetRoleAndPermissionList();
        Task<ResponseModel> EditUserRolesAndPermissionsByEmpCode(DtoEditUserRolesByEmpCode model, int empCode, string source);
        Task<ResponseModel> EditEmpRole(DtoEditEmpRole model, int empCode, string source);
        Task<ResponseModel> EditRolePermissions(DtoEditRolePermissions model, int empCode, string source);
        Task<ResponseModel> GetPermissionList();
        Task<ResponseModel> AddUserRoleMapping(AddUserRoleMapping model, int EmpCode, string source);

        Task<ResponseModel> AddNewRole(DtoAddUserRoleWithoutPermission model, int EmpCode);
        Task<ResponseModel> UpdateRole(int roleId, DtoAddUserRoleWithoutPermission model, int EmpCode);
        Task<ResponseModel> DeleteRole(int roleId, int EmpCode);
        Task<ResponseModel> GetAllRoles();
        Task<ResponseModel> GetRoleById(int roleId);
    }
}
