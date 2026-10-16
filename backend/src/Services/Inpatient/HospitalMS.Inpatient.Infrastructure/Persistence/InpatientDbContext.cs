using HospitalMS.Inpatient.Application.Common.Interfaces;
using HospitalMS.Inpatient.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalMS.Inpatient.Infrastructure.Persistence;

public class InpatientDbContext : DbContext, IInpatientDbContext
{
    public InpatientDbContext(DbContextOptions<InpatientDbContext> options) : base(options)
    {
    }

    public DbSet<DepartmentRoom> Rooms => Set<DepartmentRoom>();
    public DbSet<HospitalBed> Beds => Set<HospitalBed>();
    public DbSet<InpatientAdmission> Admissions => Set<InpatientAdmission>();
    public DbSet<BedTransferHistory> TransferHistories => Set<BedTransferHistory>();
    public DbSet<InpatientMedicalOrder> MedicalOrders => Set<InpatientMedicalOrder>();
    public DbSet<InpatientVitalSign> VitalSigns => Set<InpatientVitalSign>();
    public DbSet<InpatientCareNote> CareNotes => Set<InpatientCareNote>();
    public DbSet<InpatientAdvancePayment> AdvancePayments => Set<InpatientAdvancePayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InpatientDbContext).Assembly);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.ProviderName?.EndsWith("InMemory") == true)
        {
            return new NullScopeTransaction();
        }
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    private sealed class NullScopeTransaction : IDbContextTransaction
    {
        public Guid TransactionId => Guid.NewGuid();
        public void Commit() { }
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Rollback() { }
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

