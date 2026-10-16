using HospitalMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Common.Interfaces;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Department> Departments { get; }
    DbSet<HospitalMS.Identity.Domain.Entities.Application> Applications { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RoleApplication> RoleApplications { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
