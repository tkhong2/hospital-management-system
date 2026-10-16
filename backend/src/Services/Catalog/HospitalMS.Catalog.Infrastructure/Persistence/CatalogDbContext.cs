using System.Reflection;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Infrastructure.Persistence;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options), ICatalogDbContext
{
    public DbSet<Icd10> Icd10s => Set<Icd10>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<AdministrativeDivision> AdministrativeDivisions => Set<AdministrativeDivision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

