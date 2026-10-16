using HospitalMS.Pharmacy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.Pharmacy.Infrastructure.Persistence.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.RegistrationNumber).HasMaxLength(100);
        builder.Property(x => x.ActiveIngredient).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Concentration).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UnitName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PackageSpecification).HasMaxLength(150);
        builder.Property(x => x.RouteOfAdministration).HasMaxLength(100);
        builder.Property(x => x.MedicineGroup).HasMaxLength(150);

        builder.Property(x => x.InboundPrice).HasPrecision(18, 2);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.InsurancePrice).HasPrecision(18, 2);
        builder.Property(x => x.MinStockLevel).HasPrecision(18, 2);
        builder.Property(x => x.MaxStockLevel).HasPrecision(18, 2);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ActiveIngredient);
        builder.HasIndex(x => x.SpecialControl);
    }
}

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DepartmentName).HasMaxLength(150);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.ManagerName).HasMaxLength(150);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class InventoryBatchConfiguration : IEntityTypeConfiguration<InventoryBatch>
{
    public void Configure(EntityTypeBuilder<InventoryBatch> builder)
    {
        builder.ToTable("InventoryBatches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.InboundPrice).HasPrecision(18, 2);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.QuantityOnHand).HasPrecision(18, 2);
        builder.Property(x => x.QuantityReserved).HasPrecision(18, 2);

        // Optimistic Concurrency Token
        builder.Property(x => x.ConcurrencyVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.WarehouseId, x.MedicineId, x.BatchNumber }).IsUnique();
        builder.HasIndex(x => x.ExpiryDate);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Batches)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Medicine)
            .WithMany(x => x.Batches)
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockDocumentConfiguration : IEntityTypeConfiguration<StockDocument>
{
    public void Configure(EntityTypeBuilder<StockDocument> builder)
    {
        builder.ToTable("StockDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SupplierName).HasMaxLength(250);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.ApprovedBy).HasMaxLength(150);

        builder.HasIndex(x => x.DocumentNumber).IsUnique();
        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetWarehouse)
            .WithMany()
            .HasForeignKey(x => x.TargetWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockDocumentDetailConfiguration : IEntityTypeConfiguration<StockDocumentDetail>
{
    public void Configure(EntityTypeBuilder<StockDocumentDetail> builder)
    {
        builder.ToTable("StockDocumentDetails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);

        builder.HasOne(x => x.Document)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Medicine)
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockLedgerConfiguration : IEntityTypeConfiguration<StockLedger>
{
    public void Configure(EntityTypeBuilder<StockLedger> builder)
    {
        builder.ToTable("StockLedgers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LedgerCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DocumentNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.BalanceBefore).HasPrecision(18, 2);
        builder.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.CreatedBy).HasMaxLength(150);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => new { x.WarehouseId, x.MedicineId });
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.DocumentNumber);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Ledgers)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Medicine)
            .WithMany(x => x.Ledgers)
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrescriptionOrderConfiguration : IEntityTypeConfiguration<PrescriptionOrder>
{
    public void Configure(EntityTypeBuilder<PrescriptionOrder> builder)
    {
        builder.ToTable("PrescriptionOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrescriptionCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PatientCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DoctorName).HasMaxLength(150);
        builder.Property(x => x.Diagnosis).HasMaxLength(500);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.DispensedBy).HasMaxLength(150);

        builder.HasIndex(x => x.PrescriptionCode).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PatientId);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrescriptionOrderDetailConfiguration : IEntityTypeConfiguration<PrescriptionOrderDetail>
{
    public void Configure(EntityTypeBuilder<PrescriptionOrderDetail> builder)
    {
        builder.ToTable("PrescriptionOrderDetails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MedicineCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MedicineName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.UnitName).HasMaxLength(50);
        builder.Property(x => x.QuantityRequested).HasPrecision(18, 2);
        builder.Property(x => x.QuantityDispensed).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.UsageInstructions).HasMaxLength(500);
        builder.Property(x => x.AllocatedBatchNumber).HasMaxLength(100);

        builder.HasOne(x => x.PrescriptionOrder)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.PrescriptionOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Medicine)
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockInventoryConfiguration : IEntityTypeConfiguration<StockInventory>
{
    public void Configure(EntityTypeBuilder<StockInventory> builder)
    {
        builder.ToTable("StockInventories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InventoryCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CouncilMembers).HasMaxLength(500);
        builder.Property(x => x.BalancedBy).HasMaxLength(150);

        builder.HasIndex(x => x.InventoryCode).IsUnique();

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockInventoryDetailConfiguration : IEntityTypeConfiguration<StockInventoryDetail>
{
    public void Configure(EntityTypeBuilder<StockInventoryDetail> builder)
    {
        builder.ToTable("StockInventoryDetails");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BatchNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SystemQuantity).HasPrecision(18, 2);
        builder.Property(x => x.ActualQuantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DifferenceReason).HasMaxLength(500);

        builder.HasOne(x => x.Inventory)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Medicine)
            .WithMany()
            .HasForeignKey(x => x.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

