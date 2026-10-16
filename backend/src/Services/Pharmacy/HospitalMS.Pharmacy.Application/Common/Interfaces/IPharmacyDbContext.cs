using HospitalMS.Pharmacy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalMS.Pharmacy.Application.Common.Interfaces;

public interface IPharmacyDbContext
{
    DbSet<Medicine> Medicines { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<InventoryBatch> InventoryBatches { get; }
    DbSet<StockDocument> StockDocuments { get; }
    DbSet<StockDocumentDetail> StockDocumentDetails { get; }
    DbSet<StockLedger> StockLedgers { get; }
    DbSet<PrescriptionOrder> PrescriptionOrders { get; }
    DbSet<PrescriptionOrderDetail> PrescriptionOrderDetails { get; }
    DbSet<StockInventory> StockInventories { get; }
    DbSet<StockInventoryDetail> StockInventoryDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

