using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModuleManagementBackend.BAL.IServices;
using ModuleManagementBackend.DAL.DBContext;
using ModuleManagementBackend.DAL.DbEntities.UserEntities;
using ModuleManagementBackend.Model.Common;
using ModuleManagementBackend.Model.DTOs.UserDTO;

using System.Net;

namespace ModuleManagementBackend.BAL.Services
{
    public class UserService : IUserService
    {
        private readonly ModuleManagementDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public UserService(ModuleManagementDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        public async Task<ResponseModel> AddNewRoleWithPermission(DtoAddUserRole model, int EmpCode, string source)
        {
            var response = new ResponseModel();

            if (model != null)
            {
                var rolename = _dbContext.RoleMasters
                    .Where(r => r.RoleName.ToLower().Trim() == model.Name.ToLower().Trim())
                    .Select(r => r.RoleName)
                    .FirstOrDefault();

                if (rolename == null)
                {
                    var role = new RoleMaster();
                    role.RoleName = model.Name;
                    role.Description = model.Description;
                    role.CreatedBy = EmpCode;
                    role.CreatedDate = DateTime.Now;
                    role.CreatedSource = source;

                    await _dbContext.AddAsync(role);
                    await _dbContext.SaveChangesAsync();


                    if (model.PermissionIds.Count() > 0)
                    {
                        foreach (var item in model.PermissionIds)
                        {
                            var rolepermissionMapping = new RolePermissionMapping
                            {
                                RoleMasterId = role.Id,
                                PermissionMasterId = (int)item.PermissionId,
                                CreatedBy = EmpCode,
                                CreatedDate = DateTime.Now,
                                CreatedSource = source
                            };
                            await _dbContext.RolePermissionMappings.AddAsync(rolepermissionMapping);
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    response.Message = "New Role with Permisssions Added Sucessfully";
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    return response;

                }
                else
                {
                    response.Message = "Role with this name already exst.";
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return response;

                }

            }
            else
            {
                response.Message = "Invalid UserRole details provided.";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return response;
            }
        }
        public async Task<ResponseModel> AddUserRoleMapping(AddUserRoleMapping model, int EmpCode, string source)
        {
            var response = new ResponseModel();
            var skippedRoles = new List<string>();
            if (model != null)
            {
                var currentUserRoles = await _dbContext.userRoleMappings
                    .Where(x => x.EmpCode == model.EmpCode)
                    .Select(x => x.RoleMasterId)
                    .ToListAsync();

                foreach (var item in model.UserRoles)
                {
                    if (!currentUserRoles.Contains(item.RoleId))
                    {
                        var userRoleMapping = new UserRoleMapping
                        {
                            CreatedDate = DateTime.Now,
                            CreatedBy = EmpCode,
                            CreatedSource = source,
                            UnitId = model.EmpUnitId,
                            RoleMasterId = item.RoleId,
                            EmpCode = model.EmpCode,


                        };
                        await _dbContext.userRoleMappings.AddAsync(userRoleMapping);
                    }
                    else
                    {
                        skippedRoles.Add(item.RoleId.ToString());
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            if (skippedRoles.Any())
            {
                response.Message = $"The following roles already exist for the user: {string.Join(", ", skippedRoles)}";
                response.StatusCode = System.Net.HttpStatusCode.OK;
            }
            else
            {
                response.Message = "All roles have been successfully added.";
                response.StatusCode = System.Net.HttpStatusCode.OK;

            }

            return response;
        }
        public async Task<ResponseModel> GetRoleAndPermissionByEmpCode(int EmpCode)
        {
            var response = new ResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Invalid EmpCode"
            };

            if (EmpCode > 0)
            {
                var query = await (from RM in _dbContext.userRoleMappings
                                   join PM in _dbContext.RolePermissionMappings
                                       on RM.RoleMasterId equals PM.RoleMasterId
                                   join PRM in _dbContext.UserPermissionMasters
                                  on PM.PermissionMasterId equals PRM.Id
                                   where RM.EmpCode == EmpCode
                                   select new
                                   {
                                       RoleId = RM.RoleMasterId,
                                       PermissionId = PM.PermissionMasterId,
                                       PermissionName = PRM.Name,
                                   }).ToListAsync();

                if (query.Any())
                {
                    var groupedRolesAndPermissions = query
                        .GroupBy(x => x.RoleId)
                        .Select(g => new
                        {
                            RoleId = g.Key,
                            Permissions = g.Select(p => new
                            {
                                PermissionId = p.PermissionId,
                                PermissionName = p.PermissionName
                            }).Distinct().ToList()
                        }).ToList();

                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    response.Message = "Roles and permissions fetched successfully";
                    response.Data = groupedRolesAndPermissions;
                    response.DataLength = groupedRolesAndPermissions.Count;
                }
                else
                {
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    response.Message = "No roles or permissions found for the specified user";
                }
            }

            return response;
        }
        public async Task<ResponseModel> EditUserRolesAndPermissionsByEmpCode(DtoEditUserRolesByEmpCode model, int empCode, string source)
        {
            var response = new ResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Invalid input or employee code."
            };

            if (model == null || empCode != 0)
            {
                return response;
            }

            // Fetch existing role mappings for the user
            var existingUserRoles = await _dbContext.userRoleMappings
                .Where(urm => urm.EmpCode == empCode)
                .ToListAsync();

            // Identify roles to remove
            var rolesToRemove = existingUserRoles
                .Where(urm => !model.Roles.Any(r => r.RoleId == urm.RoleMasterId))
                .ToList();

            // Identify roles to retain (no need to modify if permissions remain unchanged)
            var rolesToRetain = existingUserRoles
                .Where(urm => model.Roles.Any(r => r.RoleId == urm.RoleMasterId))
                .ToList();

            // Identify roles to add
            var rolesToAdd = model.Roles
                .Where(r => !existingUserRoles.Any(urm => urm.RoleMasterId == r.RoleId))
                .ToList();

            // Remove roles that are no longer assigned
            if (rolesToRemove.Any())
            {
                _dbContext.userRoleMappings.RemoveRange(rolesToRemove);
            }

            // Process each role in the request
            foreach (var role in model.Roles)
            {
                // Check if role is new or already exists
                var userRoleMapping = existingUserRoles
                    .FirstOrDefault(urm => urm.RoleMasterId == role.RoleId);

                if (userRoleMapping == null)
                {
                    // Add new role mapping if it doesn't exist
                    userRoleMapping = new UserRoleMapping
                    {
                        EmpCode = empCode,
                        RoleMasterId = role.RoleId,
                        CreatedBy = empCode,
                        CreatedDate = DateTime.Now,
                        CreatedSource = source
                    };
                    await _dbContext.userRoleMappings.AddAsync(userRoleMapping);
                }

                // Update permissions for the role only if there are changes
                var existingPermissions = await _dbContext.RolePermissionMappings
                    .Where(rpm => rpm.RoleMasterId == role.RoleId)
                    .Select(rpm => rpm.PermissionMasterId)
                    .ToListAsync();

                var newPermissions = role.PermissionIds.Select(p => p.PermissionId).ToList();

                // Remove permissions that are no longer assigned
                var permissionsToRemove = existingPermissions.Except(newPermissions).ToList();
                if (permissionsToRemove.Any())
                {
                    var mappingsToRemove = _dbContext.RolePermissionMappings
                        .Where(rpm => rpm.RoleMasterId == role.RoleId && permissionsToRemove.Contains(rpm.PermissionMasterId));
                    _dbContext.RolePermissionMappings.RemoveRange(mappingsToRemove);
                }

                // Add new permissions
                var permissionsToAdd = newPermissions.Except(existingPermissions).ToList();
                foreach (var permissionId in permissionsToAdd)
                {
                    var newPermissionMapping = new RolePermissionMapping
                    {
                        RoleMasterId = role.RoleId,
                        PermissionMasterId = permissionId,
                        CreatedBy = empCode,
                        CreatedDate = DateTime.Now,
                        CreatedSource = source
                    };
                    await _dbContext.RolePermissionMappings.AddAsync(newPermissionMapping);
                }
            }

            // Save changes
            await _dbContext.SaveChangesAsync();

            response.StatusCode = System.Net.HttpStatusCode.OK;
            response.Message = "User's roles and permissions updated successfully.";
            return response;
        }
        public async Task<ResponseModel> GetRoleAndPermissionList()
        {
            var response = new ResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Invalid request"
            };

            var query = await (from RM in _dbContext.RoleMasters
                               join PM in _dbContext.RolePermissionMappings
                                   on RM.Id equals PM.RoleMasterId
                               join PRM in _dbContext.UserPermissionMasters
                                   on PM.PermissionMasterId equals PRM.Id
                               select new
                               {
                                   RoleId = RM.Id,
                                   RoleName = RM.RoleName,
                                   PermissionId = PM.PermissionMasterId,
                                   PermissionName = PRM.Name,
                               }).ToListAsync();

            if (query.Any())
            {
                var groupedRolesAndPermissions = query
                    .GroupBy(x => new { x.RoleId, x.RoleName })
                    .Select(g => new
                    {
                        RoleId = g.Key.RoleId,
                        RoleName = g.Key.RoleName,
                        Permissions = g.Select(p => new
                        {
                            PermissionId = p.PermissionId,
                            PermissionName = p.PermissionName
                        }).Distinct().ToList()
                    }).ToList();

                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Message = "Roles and permissions fetched successfully";
                response.Data = groupedRolesAndPermissions;
                response.DataLength = groupedRolesAndPermissions.Count;
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "No roles or permissions found.";
            }

            return response;
        }
        public async Task<ResponseModel> AddNewPermission(DtoAddUserPermission model, int EmpCode, string source)
        {
            var response = new ResponseModel();

            if (model != null)
            {
                var Name = _dbContext.UserPermissionMasters
                    .Where(r => r.Name.ToLower().Trim() == model.Name.ToLower().Trim())
                    .Select(r => r.Name)
                    .FirstOrDefault();

                if (Name == null)
                {
                    var Permisssion = new UserPermissionMaster();
                    Permisssion.Name = model.Name;
                    Permisssion.Description = model.Description;
                    Permisssion.CreatedBy = EmpCode;
                    Permisssion.CreatedDate = DateTime.Now;
                    Permisssion.CreatedSource = source;

                    await _dbContext.UserPermissionMasters.AddAsync(Permisssion);
                    await _dbContext.SaveChangesAsync();

                    response.Message = "New Permission Added Sucessfully";
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    return response;

                }
                else
                {
                    response.Message = "permission with this name already exst.";
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return response;

                }

            }
            else
            {
                response.Message = "Invalid Model details provided.";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return response;
            }
        }
        public async Task<ResponseModel> GetPermissionList()
        {
            var response = new ResponseModel();
            response.Message = "bad Request";
            response.StatusCode = System.Net.HttpStatusCode.BadRequest;

            var allPermission = await _dbContext.UserPermissionMasters.Select(x => new { id = x.Id, Value = x.Name }).ToListAsync();

            if (allPermission != null && allPermission.Count() > 0)
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Message = "date found";
                response.Data = allPermission;
                response.DataLength = allPermission.Count();

            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                response.Message = "no data found";
            }

            return response;
        }
        public async Task<ResponseModel> GetEmpRoleList(int unitId, int EmpCode)
        {
            string superAdmin = _configuration["SuperAdmin"];

            var response = new ResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Invalid EmpCode"
            };

            if (unitId != 0)
            {
                var baseQuery = from URM in _dbContext.userRoleMappings
                                join RM in _dbContext.RoleMasters
                                    on URM.RoleMasterId equals RM.Id
                                select new
                                {
                                    RoleId = URM.RoleMasterId,
                                    RoleName = RM.RoleName,
                                    URM.EmpCode,
                                    URM.UnitId
                                };

                if (int.TryParse(superAdmin, out int superAdminId))
                {

                    if (superAdminId != EmpCode)
                    {
                        baseQuery = baseQuery.Where(x => x.UnitId == unitId);
                    }
                }
                else
                {
                    throw new InvalidOperationException("The SuperAdmin configuration value is not a valid integer.");
                }

                var query = await baseQuery.ToListAsync();

                if (query.Any())
                {
                    var groupedEmpAndRole = query
                        .GroupBy(x => x.EmpCode)
                        .Select(g => new
                        {
                            EmpCode = g.Key,
                            Unit = g.First().UnitId,
                            Roles = g.Select(p => new
                            {
                                RoleId = p.RoleId,
                                RoleName = p.RoleName,
                            }).Distinct().ToList()
                        }).ToList();

                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    response.Message = "Roles fetched successfully";
                    response.Data = groupedEmpAndRole;
                    response.DataLength = groupedEmpAndRole.Count;
                }
                else
                {
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    response.Message = "No role listing found";
                }
            }

            return response;
        }
        public async Task<ResponseModel> EditEmpRole(DtoEditEmpRole model, int empCode, string source)
        {
            var response = new ResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Invalid input or employee code."
            };

            if (model == null || empCode == 0)
            {
                return response;
            }


            var existingUserRoles = await _dbContext.userRoleMappings
                .Where(urm => urm.EmpCode == model.EmpCode)
                .ToListAsync();

            var rolesToRemove = existingUserRoles
                .Where(urm => !model.Roles.Any(r => r.RoleId == urm.RoleMasterId))
                .ToList();

            var rolesToRetain = existingUserRoles
                .Where(urm => model.Roles.Any(r => r.RoleId == urm.RoleMasterId))
                .ToList();


            var rolesToAdd = model.Roles
                .Where(r => !existingUserRoles.Any(urm => urm.RoleMasterId == r.RoleId))
                .ToList();


            if (rolesToRemove.Any())
            {
                _dbContext.userRoleMappings.RemoveRange(rolesToRemove);
            }


            foreach (var role in model.Roles)
            {

                var userRoleMapping = existingUserRoles
                    .FirstOrDefault(urm => urm.RoleMasterId == role.RoleId);

                if (userRoleMapping == null)
                {
                    userRoleMapping = new UserRoleMapping
                    {
                        EmpCode = model.EmpCode,
                        RoleMasterId = role.RoleId,
                        UnitId = model.EmpUnitId,
                        CreatedBy = empCode,
                        CreatedDate = DateTime.Now,
                        CreatedSource = source
                    };
                    await _dbContext.userRoleMappings.AddAsync(userRoleMapping);
                }
            }

            await _dbContext.SaveChangesAsync();

            response.StatusCode = System.Net.HttpStatusCode.OK;
            response.Message = "User's roles updated successfully.";
            return response;


        }
        public async Task<ResponseModel> EditRolePermissions(DtoEditRolePermissions model, int empCode, string source)
        {
            var response = new ResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = "Invalid input or employee code."
            };

            if (model == null || empCode != 0)
            {
                return response;
            }

            var existingPermissions = await _dbContext.RolePermissionMappings
                .Where(rpm => rpm.RoleMasterId == model.RoleId)
                .Select(rpm => rpm.PermissionMasterId)
            .ToListAsync();

            var newPermissions = model.PermissionIds.Select(p => p.PermissionId).ToList();

            var permissionsToRemove = existingPermissions.Except(newPermissions).ToList();
            if (permissionsToRemove.Any())
            {
                var mappingsToRemove = _dbContext.RolePermissionMappings
                    .Where(rpm => rpm.RoleMasterId == model.RoleId && permissionsToRemove.Contains(rpm.PermissionMasterId));
                _dbContext.RolePermissionMappings.RemoveRange(mappingsToRemove);
            }

            var permissionsToAdd = newPermissions.Except(existingPermissions).ToList();
            foreach (var permissionId in permissionsToAdd)
            {
                var newPermissionMapping = new RolePermissionMapping
                {
                    RoleMasterId = model.RoleId,
                    PermissionMasterId = permissionId,
                    CreatedBy = empCode,
                    CreatedDate = DateTime.Now,
                    CreatedSource = source
                };
                await _dbContext.RolePermissionMappings.AddAsync(newPermissionMapping);
            }
            await _dbContext.SaveChangesAsync();
            response.StatusCode = System.Net.HttpStatusCode.OK;
            response.Message = "User's roles updated successfully.";
            return response;
        }
        public async Task<ResponseModel> AddNewRole(DtoAddUserRoleWithoutPermission model, int EmpCode)
        {
            var response = new ResponseModel();

            if (model != null)
            {
                var rolename = _dbContext.RoleMasters
                    .Where(r => r.RoleName.ToLower().Trim() == model.Name.ToLower().Trim())
                    .Select(r => r.RoleName)
                    .FirstOrDefault();

                if (rolename == null)
                {
                    var role = new RoleMaster();
                    role.RoleName = model.Name;
                    role.Description = model.Description;
                    role.CreatedBy = EmpCode;
                    role.CreatedDate = DateTime.Now;

                    await _dbContext.AddAsync(role);
                    await _dbContext.SaveChangesAsync();

                    response.Message = "New Role Added Sucessfully";
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    return response;

                }
                else
                {
                    response.Message = "Role with this name already exst.";
                    response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return response;

                }

            }
            else
            {
                response.Message = "Invalid UserRole details provided.";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return response;
            }
        }
        public async Task<ResponseModel> UpdateRole(int roleId, DtoAddUserRoleWithoutPermission model, int EmpCode)
        {
            var response = new ResponseModel();

            var role = await _dbContext.RoleMasters.FindAsync(roleId);
            if (role == null)
            {
                response.Message = "Role not found.";
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return response;
            }

            var existingRole = await _dbContext.RoleMasters
                .Where(r => r.RoleName.ToLower() == model.Name.ToLower() && r.Id != roleId)
                .FirstOrDefaultAsync();

            if (existingRole != null)
            {
                response.Message = "Another role with this name already exists.";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return response;
            }

            role.RoleName = model.Name;
            role.Description = model.Description;
            role.UpdatedBy = EmpCode;
            role.UpdatedDate = DateTime.Now;

            _dbContext.RoleMasters.Update(role);
            await _dbContext.SaveChangesAsync();

            response.Message = "Role updated successfully.";
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
        }
        public async Task<ResponseModel> DeleteRole(int roleId, int EmpCode)
        {
            var response = new ResponseModel();

            var role = await _dbContext.RoleMasters.FindAsync(roleId);
            if (role == null)
            {
                response.Message = "Role not found.";
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return response;
            }

            if (role.IsActive == false)
            {
                response.Message = "Role is already inactive.";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return response;
            }

            role.IsActive = false;
            role.UpdatedBy = EmpCode;
            role.UpdatedDate = DateTime.Now;

            _dbContext.RoleMasters.Update(role);
            await _dbContext.SaveChangesAsync();

            response.Message = "Role marked as inactive successfully.";
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
        }
        public async Task<ResponseModel> GetAllRoles()
        {
            var response = new ResponseModel();

            var roles = await _dbContext.RoleMasters
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            response.Data = roles;
            response.Message = roles.Any() ? "Roles retrieved successfully." : "No roles found.";
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
        }
        public async Task<ResponseModel> GetRoleById(int roleId)
        {
            var response = new ResponseModel();

            var role = await _dbContext.RoleMasters
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                response.Message = "Role not found.";
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return response;
            }

            response.Data = role;
            response.Message = "Role retrieved successfully.";
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
        }
        public async Task<ResponseModel> GetAllRolesByUnit( string role="admin")
        {
            var response = new ResponseModel();

            var roles = await _dbContext.userRoleMappings.Where(x=>x.Role.RoleName.ToLower().Trim()==role.ToLower().Trim())
                .GroupBy(y=>y.UnitId).
                Select(g => new
                {
                    UnitId = g.Key,
                    EmpCode=g.Select(x=>x.EmpCode),
                    Roles = g.Select(x => new
                    {
                        RoleId = x.RoleMasterId,
                        RoleName = x.Role.RoleName
                    }).ToList()
                })
                .ToListAsync();

            response.Data = roles;
            response.Message = roles.Any() ? "Roles retrieved successfully." : "No roles found.";
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
        }
    }
}