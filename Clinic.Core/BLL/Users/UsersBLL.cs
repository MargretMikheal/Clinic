using Clinic.Core.BLL.Users.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using Clinic.Core.Identity;
using Clinic.Core.Identity.Stores;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class UsersBLL
    {
        private readonly Entities _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersBLL()
        {
            _context = new Entities();
            var store = new ApplicationUserStore(_context);
            _userManager = new UserManager<ApplicationUser>(store);
        }

        public async Task<BaseResponse<List<UserDto>>> GetUsersAsync()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var dtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user.Id);
                    dtos.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = roles.FirstOrDefault()
                    });
                }

                return BaseResponse<List<UserDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<UserDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<UserDto>> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return BaseResponse<UserDto>.Fail("User not found.");
                }

                var roles = await _userManager.GetRolesAsync(user.Id);
                var dto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                };

                return BaseResponse<UserDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return BaseResponse<UserDto>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<UserDto>> CreateUserAsync(UserDto dto)
        {
            try
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = dto.UserName,
                    Email = dto.Email
                };

                var result = string.IsNullOrEmpty(dto.Password) 
                    ? await _userManager.CreateAsync(user) 
                    : await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    return BaseResponse<UserDto>.Fail(string.Join(", ", result.Errors));
                }

                if (!string.IsNullOrEmpty(dto.Role))
                {
                    if (dto.Role == "Doctor")
                    {
                         await _userManager.DeleteAsync(user);
                         return BaseResponse<UserDto>.Fail("Doctor role cannot be assigned from here.");
                    }
                    if (!_context.AspNetRoles.Any(r => r.Name == dto.Role))
                    {
                         return BaseResponse<UserDto>.Fail("Invalid role specified.");
                    }
                    var addRoleResult = await _userManager.AddToRoleAsync(user.Id, dto.Role);
                    if (!addRoleResult.Succeeded)
                    {
                         await _userManager.DeleteAsync(user);
                         return BaseResponse<UserDto>.Fail("Failed to assign role: " + string.Join(", ", addRoleResult.Errors));
                    }
                }
                else
                {
                    await _userManager.DeleteAsync(user);
                    return BaseResponse<UserDto>.Fail("A valid role is required.");
                }

                dto.Id = user.Id;
                dto.Password = null;
                return BaseResponse<UserDto>.Success(dto, "User created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<UserDto>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<UserDto>> UpdateUserAsync(UserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.Id);
                if (user == null)
                {
                    return BaseResponse<UserDto>.Fail("User not found.");
                }

                user.UserName = dto.UserName;
                user.Email = dto.Email;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return BaseResponse<UserDto>.Fail(string.Join(", ", result.Errors));
                }

                if (!string.IsNullOrEmpty(dto.Password))
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(dto.Password);
                    await _userManager.UpdateAsync(user);
                }

                if (!string.IsNullOrEmpty(dto.Role))
                {
                    if (!_context.AspNetRoles.Any(r => r.Name == dto.Role))
                    {
                         return BaseResponse<UserDto>.Fail("Invalid role specified.");
                    }
                    var currentRoles = await _userManager.GetRolesAsync(user.Id);
                    foreach (var role in currentRoles)
                    {
                        if (role != dto.Role)
                            await _userManager.RemoveFromRoleAsync(user.Id, role);
                    }
                    if (!currentRoles.Contains(dto.Role))
                    {
                        var addRoleResult = await _userManager.AddToRoleAsync(user.Id, dto.Role);
                        if (!addRoleResult.Succeeded)
                            return BaseResponse<UserDto>.Fail("Failed to assign role: " + string.Join(", ", addRoleResult.Errors));
                    }
                }
                else
                {
                    return BaseResponse<UserDto>.Fail("A valid role is required.");
                }

                dto.Password = null;
                return BaseResponse<UserDto>.Success(dto, "User updated successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<UserDto>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return BaseResponse<bool>.Fail("User not found.");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BaseResponse<bool>.Fail(string.Join(", ", result.Errors));
                }

                return BaseResponse<bool>.Success(true, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public BaseResponse<List<string>> GetAvailableRoles()
        {
            try
            {
                var roles = _context.AspNetRoles
                    .Where(r => r.Name != "Doctor")
                    .Select(r => r.Name).ToList();
                return BaseResponse<List<string>>.Success(roles);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<string>>.Fail(ex.Message);
            }
        }
    }
}
