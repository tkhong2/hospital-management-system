using System.Reflection;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Infrastructure.Persistence;

public class PatientDbContext(DbContextOptions<PatientDbContext> options)
    : DbContext(options), IPatientDbContext
{
    public DbSet<Domain.Entities.Patient> Patients => Set<Domain.Entities.Patient>();
    public DbSet<PatientMergeHistory> PatientMergeHistories => Set<PatientMergeHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

