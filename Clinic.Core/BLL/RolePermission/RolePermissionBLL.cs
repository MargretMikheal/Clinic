using Clinic.Core.BLL.RolePermission.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using Clinic.Core.Identity;
using Clinic.Core.Identity.Stores;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class RolePermissionBLL
    {
        private readonly Entities _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RolePermissionBLL()
        {
            _context = new Entities();
            var store = new ApplicationRoleStore(_context);
            _roleManager = new RoleManager<ApplicationRole>(store);
        }

        public async Task<BaseResponse<List<RoleListItemDto>>> GetRolesWithUserCountAsync()
        {
            try
            {
                var roleCounts = await _context.AspNetRoles
                    .Select(r => new RoleListItemDto
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        UserCount = r.AspNetUsers.Count
                    })
                    .ToListAsync();

                return BaseResponse<List<RoleListItemDto>>.Success(roleCounts);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<RoleListItemDto>>.Fail($"Error getting roles: {ex.Message}");
            }
        }

        public async Task<BaseResponse<List<RoleDto>>> GetRolesAsync()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name
                    })
                    .ToListAsync();

                return BaseResponse<List<RoleDto>>.Success(roles);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<RoleDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<RoleDto>> CreateRoleAsync(RoleDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return BaseResponse<RoleDto>.Fail("Role name is required.");
                }

                var existingRole = await _roleManager.FindByNameAsync(dto.Name);
                if (existingRole != null)
                {
                    return BaseResponse<RoleDto>.Fail("Duplicate role.");
                }

                var role = new ApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    return BaseResponse<RoleDto>.Fail(string.Join(", ", result.Errors));
                }

                dto.Id = role.Id;
                return BaseResponse<RoleDto>.Success(dto, "Role added successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<RoleDto>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<List<ScreenPermissionDto>>> GetRolePermissionsAsync(string roleId)
        {
            try
            {
                var dtos = await (
                 from screen in _context.Screens
                 join permission in _context.RoleScreenPermissions
                     .Where(rp => rp.RoleId == roleId)
                     on screen.Id equals permission.ScreenId into permissions
                 from permission in permissions.DefaultIfEmpty()
                 where screen.IsActive && !screen.IsDeleted
                 select new ScreenPermissionDto
                 {
                     ScreenId = screen.Id,
                     ScreenName = screen.Name,
                     Area = screen.Area,
                     Controller = screen.Controller,
                     Action = screen.Action,
                     Icon = screen.Icon,
                     ParentId = screen.ParentId,

                     ScreenShow = permission != null && permission.ScreenShow,
                     ScreenAdd = permission != null && permission.ScreenAdd,
                     ScreenEdit = permission != null && permission.ScreenEdit,
                     ScreenDelete = permission != null && permission.ScreenDelete,
                     ScreenPrint = permission != null && permission.ScreenPrint
                 }
             ).ToListAsync();

                return BaseResponse<List<ScreenPermissionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<ScreenPermissionDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> SaveRolePermissionsAsync(
             SaveRoleScreenPermissionDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.RoleId))
                {
                    return BaseResponse<bool>.Fail("Role is required.");
                }

                var currentUserName =
                    Thread.CurrentPrincipal?.Identity?.Name ?? "Admin";

                var currentDate = DateTime.Now;

                var existingPermissions = await _context.RoleScreenPermissions
                    .Where(rp => rp.RoleId == dto.RoleId)
                    .ToDictionaryAsync(rp => rp.ScreenId);

                foreach (var permDto in dto.Permissions)
                {
                    var hasAnyPermission =
                        permDto.ScreenShow ||
                        permDto.ScreenAdd ||
                        permDto.ScreenEdit ||
                        permDto.ScreenDelete ||
                        permDto.ScreenPrint;

                    existingPermissions.TryGetValue(
                        permDto.ScreenId,
                        out var existingRecord);

                    if (hasAnyPermission)
                    {
                        if (existingRecord == null)
                        {
                            var newPermission = new RoleScreenPermission
                            {
                                RoleId = dto.RoleId,
                                ScreenId = permDto.ScreenId,
                                ScreenShow = permDto.ScreenShow,
                                ScreenAdd = permDto.ScreenAdd,
                                ScreenEdit = permDto.ScreenEdit,
                                ScreenDelete = permDto.ScreenDelete,
                                ScreenPrint = permDto.ScreenPrint,
                                CreatedDate = currentDate,
                                CreatedBy = currentUserName
                            };

                            _context.RoleScreenPermissions.Add(newPermission);
                        }
                        else
                        {
                            existingRecord.ScreenShow = permDto.ScreenShow;
                            existingRecord.ScreenAdd = permDto.ScreenAdd;
                            existingRecord.ScreenEdit = permDto.ScreenEdit;
                            existingRecord.ScreenDelete = permDto.ScreenDelete;
                            existingRecord.ScreenPrint = permDto.ScreenPrint;
                            existingRecord.ModifiedDate = currentDate;
                            existingRecord.ModifiedBy = currentUserName;
                        }
                    }
                    else if (existingRecord != null)
                    {
                        _context.RoleScreenPermissions.Remove(existingRecord);
                    }
                }

                await _context.SaveChangesAsync();

                return BaseResponse<bool>.Success(
                    true,
                    "Permissions saved successfully");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }
    }
}
