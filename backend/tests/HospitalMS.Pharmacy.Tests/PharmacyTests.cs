using FluentAssertions;
using HospitalMS.Pharmacy.Application.Features.Prescriptions.Commands;
using HospitalMS.Pharmacy.Application.Features.StockDocuments.Commands;
using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using HospitalMS.Pharmacy.Domain.Services;
using HospitalMS.Pharmacy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HospitalMS.Pharmacy.Tests;

public class PharmacyTests
{
    private PharmacyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PharmacyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new PharmacyDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void FefoAllocation_AllocatesFromEarliestExpiringBatchFirst()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var batches = new List<InventoryBatch>
        {
            new() { Id = Guid.NewGuid(), BatchNumber = "BATCH-LATER", ExpiryDate = now.AddMonths(12), QuantityOnHand = 100, SalePrice = 1000 },
            new() { Id = Guid.NewGuid(), BatchNumber = "BATCH-SOONER", ExpiryDate = now.AddMonths(2), QuantityOnHand = 50, SalePrice = 1000 }
        };

        // Act
        var allocations = FefoAllocationService.Allocate(batches, requiredQuantity: 30, currentDate: now);

        // Assert
        allocations.Should().HaveCount(1);
        allocations[0].BatchNumber.Should().Be("BATCH-SOONER");
        allocations[0].AllocatedQuantity.Should().Be(30);
    }

    [Fact]
    public void FefoAllocation_SplitsAcrossMultipleBatches_WhenFirstBatchInsufficient()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var batch1 = new InventoryBatch { Id = Guid.NewGuid(), BatchNumber = "BATCH-1", ExpiryDate = now.AddMonths(1), QuantityOnHand = 40, SalePrice = 1000 };
        var batch2 = new InventoryBatch { Id = Guid.NewGuid(), BatchNumber = "BATCH-2", ExpiryDate = now.AddMonths(6), QuantityOnHand = 100, SalePrice = 1000 };
        var batches = new List<InventoryBatch> { batch2, batch1 };

        // Act: cần xuất 70 viên (Batch 1 có 40 -> lấy hết 40, còn lại 30 lấy từ Batch 2)
        var allocations = FefoAllocationService.Allocate(batches, requiredQuantity: 70, currentDate: now);

        // Assert
        allocations.Should().HaveCount(2);
        allocations[0].BatchNumber.Should().Be("BATCH-1");
        allocations[0].AllocatedQuantity.Should().Be(40);

        allocations[1].BatchNumber.Should().Be("BATCH-2");
        allocations[1].AllocatedQuantity.Should().Be(30);
    }

    [Fact]
    public void FefoAllocation_ThrowsException_WhenTotalStockInsufficient()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var batches = new List<InventoryBatch>
        {
            new() { Id = Guid.NewGuid(), BatchNumber = "BATCH-1", ExpiryDate = now.AddMonths(2), QuantityOnHand = 20 }
        };

        // Act & Assert
        var act = () => FefoAllocationService.Allocate(batches, requiredQuantity: 50, currentDate: now);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Không đủ tồn kho khả dụng*");
    }

    [Fact]
    public void InventoryBatch_DeductStock_IncrementsConcurrencyVersion()
    {
        // Arrange
        var batch = new InventoryBatch
        {
            BatchNumber = "BATCH-01",
            QuantityOnHand = 100,
            ConcurrencyVersion = 1
        };

        // Act
        batch.DeductStock(30);

        // Assert
        batch.QuantityOnHand.Should().Be(70);
        batch.ConcurrencyVersion.Should().Be(2);
    }

    [Fact]
    public async Task DbInitializer_SeedsPharmacyDataProperly()
    {
        using var context = CreateDbContext();
        await PharmacyDbInitializer.SeedAsync(context);

        var warehouses = await context.Warehouses.ToListAsync();
        warehouses.Should().HaveCount(3);

        var medicines = await context.Medicines.ToListAsync();
        medicines.Should().HaveCount(6);

        var batches = await context.InventoryBatches.ToListAsync();
        batches.Should().HaveCount(6);

        var ledgers = await context.StockLedgers.ToListAsync();
        ledgers.Should().HaveCount(2);
    }

    [Fact]
    public async Task ApproveInboundDocument_CreatesBatchAndWritesStockLedger()
    {
        // Arrange
        using var context = CreateDbContext();
        var warehouse = new Warehouse { Code = "K1", Name = "Kho Test" };
        var medicine = new Medicine { Code = "M1", Name = "Thuốc Test", ActiveIngredient = "Para", Concentration = "500", UnitName = "Viên" };
        context.Warehouses.Add(warehouse);
        context.Medicines.Add(medicine);
        await context.SaveChangesAsync();

        var doc = new StockDocument
        {
            DocumentNumber = "PN-TEST-01",
            DocumentType = DocumentType.Inbound,
            TargetWarehouseId = warehouse.Id,
            Status = DocumentStatus.Draft,
            TotalAmount = 500000
        };
        doc.Details.Add(new StockDocumentDetail
        {
            MedicineId = medicine.Id,
            BatchNumber = "LOTE-001",
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            Quantity = 500,
            UnitPrice = 1000
        });
        context.StockDocuments.Add(doc);
        await context.SaveChangesAsync();

        var handler = new ApproveStockDocumentCommandHandler(context);

        // Act
        var result = await handler.Handle(new ApproveStockDocumentCommand(doc.Id, "Dược sĩ A"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var batch = await context.InventoryBatches.FirstOrDefaultAsync(b => b.BatchNumber == "LOTE-001");
        batch.Should().NotBeNull();
        batch!.QuantityOnHand.Should().Be(500);

        var ledger = await context.StockLedgers.FirstOrDefaultAsync(l => l.DocumentNumber == "PN-TEST-01");
        ledger.Should().NotBeNull();
        ledger!.Quantity.Should().Be(500);
        ledger.BalanceAfter.Should().Be(500);
    }

    [Fact]
    public async Task DispensePrescription_DeductsBatchUsingFefoAndWritesStockLedger()
    {
        // Arrange
        using var context = CreateDbContext();
        await PharmacyDbInitializer.SeedAsync(context);

        var pendingOrder = await context.PrescriptionOrders
            .Include(x => x.Details)
            .FirstAsync(x => x.Status == PrescriptionStatus.Pending);

        var handler = new DispensePrescriptionCommandHandler(context);

        // Act: Cấp phát đơn thuốc
        var result = await handler.Handle(new DispensePrescriptionCommand(pendingOrder.Id, pendingOrder.WarehouseId, "DS. Test"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedOrder = await context.PrescriptionOrders.FindAsync(pendingOrder.Id);
        updatedOrder!.Status.Should().Be(PrescriptionStatus.Dispensed);
        updatedOrder.DispensedBy.Should().Be("DS. Test");

        var ledger = await context.StockLedgers
            .FirstOrDefaultAsync(l => l.DocumentNumber == pendingOrder.PrescriptionCode);
        ledger.Should().NotBeNull();
        ledger!.Quantity.Should().Be(-20); // Đã xuất 20 viên
        ledger.TransactionType.Should().Be(LedgerTransactionType.PrescriptionDispense);
    }
}

