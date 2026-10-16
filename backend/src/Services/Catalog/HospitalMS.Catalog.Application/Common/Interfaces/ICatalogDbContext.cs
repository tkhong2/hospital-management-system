using HospitalMS.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Application.Common.Interfaces;

public interface ICatalogDbContext
{
    DbSet<Icd10> Icd10s { get; }
    DbSet<MedicalService> MedicalServices { get; }
    DbSet<Department> Departments { get; }
    DbSet<Specialty> Specialties { get; }
    DbSet<Unit> Units { get; }
    DbSet<AdministrativeDivision> AdministrativeDivisions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}

