using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Clinic.Core.Data;

namespace Clinic.Core.Identity.Stores
{
    public class ApplicationRoleStore : 
        IRoleStore<ApplicationRole>,
        IQueryableRoleStore<ApplicationRole>
    {
        private readonly Entities _context;

        public ApplicationRoleStore(Entities context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IQueryable<ApplicationRole> Roles => _context.AspNetRoles.Select(r => new ApplicationRole { Id = r.Id, Name = r.Name });

        public Task CreateAsync(ApplicationRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            _context.AspNetRoles.Add(new AspNetRole
            {
                Id = role.Id,
                Name = role.Name
            });
            return _context.SaveChangesAsync();
        }

        public Task DeleteAsync(ApplicationRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            var entity = _context.AspNetRoles.Find(role.Id);
            if (entity != null)
            {
                _context.AspNetRoles.Remove(entity);
                return _context.SaveChangesAsync();
            }
            return Task.FromResult(0);
        }

        public Task<ApplicationRole> FindByIdAsync(string roleId)
        {
            var role = _context.AspNetRoles.Find(roleId);
            if (role != null)
            {
                return Task.FromResult(new ApplicationRole { Id = role.Id, Name = role.Name });
            }
            return Task.FromResult<ApplicationRole>(null);
        }

        public Task<ApplicationRole> FindByNameAsync(string roleName)
        {
            var role = _context.AspNetRoles.FirstOrDefault(r => r.Name == roleName);
            if (role != null)
            {
                return Task.FromResult(new ApplicationRole { Id = role.Id, Name = role.Name });
            }
            return Task.FromResult<ApplicationRole>(null);
        }

        public Task UpdateAsync(ApplicationRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            var entity = _context.AspNetRoles.Find(role.Id);
            if (entity != null)
            {
                entity.Name = role.Name;
                return _context.SaveChangesAsync();
            }
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }
    }
}
