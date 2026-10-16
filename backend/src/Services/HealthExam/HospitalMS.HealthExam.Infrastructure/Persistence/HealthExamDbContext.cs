using System.Reflection;
using HospitalMS.HealthExam.Application.Common.Interfaces;
using HospitalMS.HealthExam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.HealthExam.Infrastructure.Persistence;

public class HealthExamDbContext(DbContextOptions<HealthExamDbContext> options)
    : DbContext(options), IHealthExamDbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<ExamContract> ExamContracts => Set<ExamContract>();
    public DbSet<ContractPayment> ContractPayments => Set<ContractPayment>();
    public DbSet<ExamPackage> ExamPackages => Set<ExamPackage>();
    public DbSet<PackageService> PackageServices => Set<PackageService>();
    public DbSet<ExamRegistration> ExamRegistrations => Set<ExamRegistration>();
    public DbSet<ExamVisit> ExamVisits => Set<ExamVisit>();
    public DbSet<ExamStation> ExamStations => Set<ExamStation>();
    public DbSet<StationResult> StationResults => Set<StationResult>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<ReferenceRange> ReferenceRanges => Set<ReferenceRange>();
    public DbSet<ExamConclusion> ExamConclusions => Set<ExamConclusion>();
    public DbSet<HealthCertificate> HealthCertificates => Set<HealthCertificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

