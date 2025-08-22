using ModuleManagementBackend.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModuleManagementBackend.BAL.Services.PoliciesService;
using static ModuleManagementBackend.Model.DTOs.PoliciesGenricDTO.PoliciesCommonDTO;

namespace ModuleManagementBackend.BAL.IServices
{
    public interface IPolicyService
    {
        Task<ResponseModel> GetAllPolicies(bool onlyWhatNew = false);
        Task<ResponseModel> AddPolicy(AddPolicyDto dto,string LoginUserEmpCode);
        Task<ResponseModel> UpdatePolicy(int id, UpdatePolicyDto dto, string LoginUserEmpCode);
        Task<ResponseModel> DeletePolicy(int id);

       
        Task<ResponseModel> AddPolicyItem(AddPolicyItemDto dto, string LoginUserEmpCode);
        Task<ResponseModel> UpdatePolicyItem(int id, UpdatePolicyItemDto dto, string LoginUserEmpCode);
        Task<ResponseModel> DeletePolicyItem(int id);
        Task<FileResponseModel> DownloadPolicyAsync(int policyItemId, string employeeId);
        Task<ResponseModel> GetPolicyDataAsync(int? mode = null);
    }

}
