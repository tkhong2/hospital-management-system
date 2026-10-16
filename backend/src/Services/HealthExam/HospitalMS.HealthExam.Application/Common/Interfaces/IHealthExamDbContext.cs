using HospitalMS.HealthExam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.HealthExam.Application.Common.Interfaces;

public interface IHealthExamDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<ExamContract> ExamContracts { get; }
    DbSet<ContractPayment> ContractPayments { get; }
    DbSet<ExamPackage> ExamPackages { get; }
    DbSet<PackageService> PackageServices { get; }
    DbSet<ExamRegistration> ExamRegistrations { get; }
    DbSet<ExamVisit> ExamVisits { get; }
    DbSet<ExamStation> ExamStations { get; }
    DbSet<StationResult> StationResults { get; }
    DbSet<LabOrder> LabOrders { get; }
    DbSet<LabResult> LabResults { get; }
    DbSet<ReferenceRange> ReferenceRanges { get; }
    DbSet<ExamConclusion> ExamConclusions { get; }
    DbSet<HealthCertificate> HealthCertificates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

