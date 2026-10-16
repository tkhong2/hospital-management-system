using HospitalMS.Outpatient.Application.Common.Interfaces;
using HospitalMS.Outpatient.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalMS.Outpatient.Infrastructure.Persistence;

public class OutpatientDbContext : DbContext, IOutpatientDbContext
{
    public OutpatientDbContext(DbContextOptions<OutpatientDbContext> options) : base(options)
    {
    }

    public DbSet<OutpatientReception> Receptions => Set<OutpatientReception>();
    public DbSet<OutpatientEncounter> Encounters => Set<OutpatientEncounter>();
    public DbSet<OutpatientServiceOrder> ServiceOrders => Set<OutpatientServiceOrder>();
    public DbSet<OutpatientServiceOrderDetail> ServiceOrderDetails => Set<OutpatientServiceOrderDetail>();
    public DbSet<OutpatientPrescription> Prescriptions => Set<OutpatientPrescription>();
    public DbSet<OutpatientPrescriptionItem> PrescriptionItems => Set<OutpatientPrescriptionItem>();
    public DbSet<OutpatientBilling> Billings => Set<OutpatientBilling>();
    public DbSet<OutpatientBillingItem> BillingItems => Set<OutpatientBillingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutpatientDbContext).Assembly);
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

