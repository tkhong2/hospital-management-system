using HospitalMS.Patient.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Common.Interfaces;

public interface IPatientDbContext
{
    DbSet<Domain.Entities.Patient> Patients { get; }
    DbSet<PatientMergeHistory> PatientMergeHistories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPatientCodeGenerator
{
    Task<string> GenerateCodeAsync(CancellationToken cancellationToken = default);
}

