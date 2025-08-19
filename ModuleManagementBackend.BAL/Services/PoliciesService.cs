using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.Models;
using ModuleManagementBackend.Model.Common;
using System.Net;
using static ModuleManagementBackend.Model.DTOs.PoliciesGenricDTO.PoliciesCommonDTO;

namespace ModuleManagementBackend.BAL.Services
{
    public class PoliciesService : IPolicyService
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
                var allPolicies = await context.tblPolices.Where(x => x.status == 0).ToListAsync();
                var allItems = await context.tblPolicyItems.Where(x => x.status == 0).ToListAsync();

               
                var itemsLookup = allItems
                    .GroupBy(i => i.fkPolId.Value)
                    .ToDictionary(g => g.Key, g => g.OrderBy(i => i.OrderFactor).ToList());

                
                List<PolicyDto> BuildTree(int parentId)
                {
                    return allPolicies
                        .Where(p => p.ParentPolicyId == parentId)
                        .Select(p => new PolicyDto
                        {
                            pkPolId = p.pkPolId,
                            PolicyHead = p.PolicyHead,
                            Children = BuildTree(p.pkPolId),
                            PolicyItems = itemsLookup.TryGetValue(p.pkPolId, out var policyItems)
                                ? policyItems.Select(i => new PolicyItemDto
                                {
                                    pkPolItemId = i.pkPolItemId,
                                    itemSubject = i.itemSubject,
                                    itemContent = i.itemContent,
                                    itemDescription = i.itemDescription,
                                    itemType = i.itemType,
                                    docName = i.docName,
                                    fileName = i.filName,
                                    OrderFactor = i.OrderFactor
                                    // Url = !string.IsNullOrEmpty(i.filName) ? $"{PoliciesPath}{i.pkPolItemId}" : ""
                                }).ToList()
                                : new List<PolicyItemDto>()
                        })
                        .ToList();
                }

                var result = BuildTree(0);

                response.Message = "Policies fetched successfully.";
                response.StatusCode = HttpStatusCode.OK;
                response.Data = result;
                response.TotalRecords = result.Count;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        public async Task<ResponseModel> AddPolicy(AddPolicyDto dto, string loginUserEmpCode)
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

        public async Task<ResponseModel> UpdatePolicy(int id, UpdatePolicyDto dto, string LoginUserEmpCode)
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

       

        #endregion
        #region Policy Items Methods

        public async Task<ResponseModel> AddPolicyItem(AddPolicyItemDto dto, string LoginUserEmpCode)
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
                    createBy = LoginUserEmpCode,
                    createdate = DateTime.Now,
                    modifyBy = LoginUserEmpCode,
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

        public async Task<ResponseModel> UpdatePolicyItem(int id, UpdatePolicyItemDto dto, string loginUserEmpCode)
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
                item.status = 0;
                item.modifyBy =loginUserEmpCode;
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
