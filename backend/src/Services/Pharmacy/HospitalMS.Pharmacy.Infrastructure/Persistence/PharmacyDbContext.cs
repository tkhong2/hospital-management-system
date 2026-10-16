using System.Reflection;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalMS.Pharmacy.Infrastructure.Persistence;

public class PharmacyDbContext(DbContextOptions<PharmacyDbContext> options)
    : DbContext(options), IPharmacyDbContext
{
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryBatch> InventoryBatches => Set<InventoryBatch>();
    public DbSet<StockDocument> StockDocuments => Set<StockDocument>();
    public DbSet<StockDocumentDetail> StockDocumentDetails => Set<StockDocumentDetail>();
    public DbSet<StockLedger> StockLedgers => Set<StockLedger>();
    public DbSet<PrescriptionOrder> PrescriptionOrders => Set<PrescriptionOrder>();
    public DbSet<PrescriptionOrderDetail> PrescriptionOrderDetails => Set<PrescriptionOrderDetail>();
    public DbSet<StockInventory> StockInventories => Set<StockInventory>();
    public DbSet<StockInventoryDetail> StockInventoryDetails => Set<StockInventoryDetail>();

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.ProviderName?.EndsWith("InMemory", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new NullScopeTransaction();
        }

        return await Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

internal class NullScopeTransaction : IDbContextTransaction
{
    public Guid TransactionId { get; } = Guid.NewGuid();
    public void Commit() { }
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Rollback() { }
    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

