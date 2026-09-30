using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Clinic.Core.Data;

namespace Clinic.Core.Identity.Stores
{
    public class ApplicationUserStore :
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserSecurityStampStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserClaimStore<ApplicationUser>,
        IQueryableUserStore<ApplicationUser>
    {
        private readonly Entities _context;

        public ApplicationUserStore(Entities context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private ApplicationUser Map(AspNetUser u)
        {
            if (u == null) return null;
            return new ApplicationUser
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                PasswordHash = u.PasswordHash,
                SecurityStamp = u.SecurityStamp,
                PhoneNumber = u.PhoneNumber,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled,
                LockoutEndDateUtc = u.LockoutEndDateUtc,
                LockoutEnabled = u.LockoutEnabled,
                AccessFailedCount = u.AccessFailedCount
            };
        }

        public IQueryable<ApplicationUser> Users => _context.AspNetUsers.Select(u => new ApplicationUser
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            EmailConfirmed = u.EmailConfirmed,
            PasswordHash = u.PasswordHash,
            SecurityStamp = u.SecurityStamp,
            PhoneNumber = u.PhoneNumber,
            PhoneNumberConfirmed = u.PhoneNumberConfirmed,
            TwoFactorEnabled = u.TwoFactorEnabled,
            LockoutEndDateUtc = u.LockoutEndDateUtc,
            LockoutEnabled = u.LockoutEnabled,
            AccessFailedCount = u.AccessFailedCount
        });

        // IUserStore
        public Task CreateAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var entity = new AspNetUser
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEndDateUtc = user.LockoutEndDateUtc,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount
            };
            _context.AspNetUsers.Add(entity);
            return _context.SaveChangesAsync();
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var entity = _context.AspNetUsers.Find(user.Id);
            if (entity != null)
            {
                _context.AspNetUsers.Remove(entity);
                return _context.SaveChangesAsync();
            }
            return Task.FromResult(0);
        }

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            return Task.FromResult(Map(_context.AspNetUsers.Find(userId)));
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(Map(_context.AspNetUsers.FirstOrDefault(u => u.UserName == userName)));
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var entity = _context.AspNetUsers.Find(user.Id);
            if (entity != null)
            {
                entity.UserName = user.UserName;
                entity.Email = user.Email;
                entity.EmailConfirmed = user.EmailConfirmed;
                entity.PasswordHash = user.PasswordHash;
                entity.SecurityStamp = user.SecurityStamp;
                entity.PhoneNumber = user.PhoneNumber;
                entity.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                entity.TwoFactorEnabled = user.TwoFactorEnabled;
                entity.LockoutEndDateUtc = user.LockoutEndDateUtc;
                entity.LockoutEnabled = user.LockoutEnabled;
                entity.AccessFailedCount = user.AccessFailedCount;
                return _context.SaveChangesAsync();
            }
            return Task.FromResult(0);
        }

        // IUserPasswordStore
        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        // IUserEmailStore
        public Task SetEmailAsync(ApplicationUser user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(ApplicationUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return Task.FromResult(Map(_context.AspNetUsers.FirstOrDefault(u => u.Email == email)));
        }

        // IUserSecurityStampStore
        public Task SetSecurityStampAsync(ApplicationUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(ApplicationUser user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        // IUserRoleStore
        public Task AddToRoleAsync(ApplicationUser user, string roleName)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var entity = _context.AspNetUsers.Find(user.Id);
            var role = _context.AspNetRoles.FirstOrDefault(r => r.Name == roleName);
            if (entity != null && role != null)
            {
                if (!entity.AspNetRoles.Contains(role))
                {
                    entity.AspNetRoles.Add(role);
                    return _context.SaveChangesAsync();
                }
            }
            return Task.FromResult(0);
        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var entity = _context.AspNetUsers.Find(user.Id);
            var role = _context.AspNetRoles.FirstOrDefault(r => r.Name == roleName);
            if (entity != null && role != null)
            {
                if (entity.AspNetRoles.Contains(role))
                {
                    entity.AspNetRoles.Remove(role);
                    return _context.SaveChangesAsync();
                }
            }
            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var roles = _context.AspNetUsers
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.AspNetRoles.Select(r => r.Name))
                .ToList();

            return Task.FromResult<IList<string>>(roles);
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            bool isInRole = _context.AspNetUsers
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.AspNetRoles)
                .Any(r => r.Name == roleName);

            return Task.FromResult(isInRole);
        }

        // IUserClaimStore
        public Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var entity = _context.AspNetUsers.Find(user.Id);
            if (entity != null)
            {
                IList<Claim> claims = entity.AspNetUserClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
                return Task.FromResult(claims);
            }
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public Task AddClaimAsync(ApplicationUser user, Claim claim)
        {
            var entity = _context.AspNetUsers.Find(user.Id);
            if (entity != null)
            {
                entity.AspNetUserClaims.Add(new AspNetUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    UserId = user.Id
                });
                return _context.SaveChangesAsync();
            }
            return Task.FromResult(0);
        }

        public Task RemoveClaimAsync(ApplicationUser user, Claim claim)
        {
            var entity = _context.AspNetUsers.Find(user.Id);
            if (entity != null)
            {
                var userClaim = entity.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
                if (userClaim != null)
                {
                    entity.AspNetUserClaims.Remove(userClaim);
                    return _context.SaveChangesAsync();
                }
            }
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }
    }
}
