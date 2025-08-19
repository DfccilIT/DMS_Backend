using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using System.Net;
using static ModuleManagementBackend.Model.DTOs.PoliciesGenricDTO.PoliciesCommonDTO;

namespace ModuleManagementBackend.BAL.Services
{
    public class PoliciesService
    {
        private readonly SAPTOKENContext context;

        public PoliciesService(SAPTOKENContext context)
        {
            this.context=context;
        }

        #region Policies Methods
        public async Task<ResponseModel> GetAllPolicies()
        {
            var response = new ResponseModel();
            try
            {
                var flatPolicies = await context.tblPolices
                    .Select(x => new GETPolicyDto
                    {
                        PkPolId = x.pkPolId,
                        PolicyHead = x.PolicyHead,
                        ParentPolicyId = x.ParentPolicyId,
                        Status = x.status.Value,
                        CreateBy = x.createBy,
                        CreateDate = x.createdate,
                        ModifyBy = x.modifyBy,
                        ModifyDate = x.modifydate
                    })
                    .ToListAsync();

                var policyTree = BuildPolicyTree(flatPolicies);

                response.Message = "Policies fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = policyTree;
                response.TotalRecords = flatPolicies.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<ResponseModel> AddPolicy(PolicyDto dto, string loginUserEmpCode)
        {
            var response = new ResponseModel();
            try
            {
                if (dto.ParentPolicyId != 0)
                {
                    var parentExists = await context.tblPolices.AnyAsync(p => p.pkPolId == dto.ParentPolicyId);
                    if (!parentExists)
                    {
                        response.Message = "Parent policy not found.";
                        response.StatusCode = HttpStatusCode.BadRequest;
                        return response;
                    }
                }

                var policy = new tblPolice
                {
                    PolicyHead = dto.PolicyHead,
                    ParentPolicyId = dto.ParentPolicyId,
                    status = 0,
                    createBy = loginUserEmpCode,
                    createdate = DateTime.Now

                };

                await context.tblPolices.AddAsync(policy);
                await context.SaveChangesAsync();

                response.Message = "Policy added successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = policy;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseModel> UpdatePolicy(int id, PolicyDto dto, string LoginUserEmpCode)
        {
            var response = new ResponseModel();
            try
            {
                var policy = await context.tblPolices.FirstOrDefaultAsync(x => x.pkPolId==id);
                if (policy == null)
                {
                    response.Message = "Policy not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                policy.PolicyHead = dto.PolicyHead;
                policy.ParentPolicyId = dto.ParentPolicyId;
                policy.modifyBy = LoginUserEmpCode;
                policy.modifydate = DateTime.Now;

                await context.SaveChangesAsync();

                response.Message = "Policy updated successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = policy;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseModel> DeletePolicy(int id)
        {
            var response = new ResponseModel();
            try
            {
                var policy = await context.tblPolices.FirstOrDefaultAsync(x => x.pkPolId==id && x.status==0);
                if (policy == null)
                {
                    response.Message = "Policy not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                policy.status=9;
                await context.SaveChangesAsync();

                response.Message = "Policy deleted successfully.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        private List<GETPolicyDto> BuildPolicyTree(List<GETPolicyDto> policies, int? parentId = 0)
        {
            return policies
                .Where(p => p.ParentPolicyId == parentId)
                .Select(p => new GETPolicyDto
                {
                    PkPolId = p.PkPolId,
                    PolicyHead = p.PolicyHead,
                    ParentPolicyId = p.ParentPolicyId,
                    Status = p.Status,
                    CreateBy = p.CreateBy,
                    CreateDate = p.CreateDate,
                    ModifyBy = p.ModifyBy,
                    ModifyDate = p.ModifyDate,

                    Children = BuildPolicyTree(policies, p.PkPolId)
                })
                .ToList();
        }

        #endregion
        #region Policy Items Methods

        public async Task<ResponseModel> AddPolicyItem(AddPolicyItemDto dto)
        {
            var response = new ResponseModel();
            try
            {
                string fileName = string.Empty;
                string docName = string.Empty;
                string fileExtension = string.Empty;
                if (dto.Doc != null && dto.Doc.Length > 0)
                {
                    fileName = await UploadPolicyDocIfAvailable(dto.Doc);
                    fileExtension = Path.GetExtension(dto.Doc.FileName);
                    docName = dto.Doc.FileName;
                }

                var item = new tblPolicyItem
                {
                    fkPolId = dto.FkPolId,
                    itemSubject = dto.ItemSubject,
                    itemType = dto.ItemType,
                    itemContent = dto.ItemContent,
                    itemDescription = dto.ItemDescription,
                    docName = docName,
                    docExtension = fileExtension,
                    filName = fileName,
                    officeOrderDate = dto.OfficeOrderDate,
                    OrderFactor = dto.OrderFactor,
                    status = 0,
                    createBy = dto.CreateBy,
                    createdate = DateTime.Now,
                    modifyBy = dto.CreateBy,
                    modifydate = DateTime.Now
                };

                await context.tblPolicyItems.AddAsync(item);
                await context.SaveChangesAsync();

                response.Message = "Policy item added successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = item;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseModel> UpdatePolicyItem(int id, UpdatePolicyItemDto dto)
        {
            var response = new ResponseModel();
            try
            {
                var item = await context.tblPolicyItems.FindAsync(id);
                if (item == null)
                {
                    response.Message = "Policy item not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                if (dto.Doc != null && dto.Doc.Length > 0)
                {
                    item.filName = await UploadPolicyDocIfAvailable(dto.Doc);
                    item.docName = dto.Doc.FileName;
                    item.docExtension = Path.GetExtension(dto.Doc.FileName);
                }

                item.itemSubject = dto.ItemSubject;
                item.itemType = dto.ItemType;
                item.itemContent = dto.ItemContent;
                item.itemDescription = dto.ItemDescription;
                item.officeOrderDate = dto.OfficeOrderDate;
                item.OrderFactor = dto.OrderFactor;
                item.status = dto.Status;
                item.modifyBy = dto.ModifyBy;
                item.modifydate = DateTime.Now;

                await context.SaveChangesAsync();

                response.Message = "Policy item updated successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = item;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        public async Task<ResponseModel> DeletePolicyItem(int id)
        {
            var response = new ResponseModel();
            try
            {
                var item = await context.tblPolicyItems.FirstOrDefaultAsync(x => x.pkPolItemId==id && x.status==0);
                if (item == null)
                {
                    response.Message = "Policy item not found.";
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                item.status = 9;
                await context.SaveChangesAsync();

                response.Message = "Policy item deleted successfully.";
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        private async Task<string> UploadPolicyDocIfAvailable(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;


            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PolicyDocs");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);


            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return uniqueFileName;
        }


        #endregion


    }
}
