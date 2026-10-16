using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Infrastructure.Persistence;

public static class PharmacyDbInitializer
{
    public static async Task SeedAsync(PharmacyDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Warehouses.AnyAsync())
        {
            return; // Đã có dữ liệu
        }

        // 1. Kho Dược (Warehouses)
        var whMain = new Warehouse
        {
            Code = "KC01",
            Name = "Kho Chẵn Dược Phẩm Trung Tâm",
            WarehouseType = WarehouseType.MainWarehouse,
            Location = "Tòa nhà Dược - Tầng 1",
            ManagerName = "DSCKI. Vũ Thu Hà",
            IsActive = true
        };

        var whOutpatient = new Warehouse
        {
            Code = "KL01",
            Name = "Kho Lẻ Phát Thuốc Ngoại Trú",
            WarehouseType = WarehouseType.SubWarehouse,
            Location = "Khu Khám bệnh Ngoại trú - Quầy 1-4",
            ManagerName = "DS. Đặng Minh Quân",
            IsActive = true
        };

        var whIcu = new Warehouse
        {
            Code = "TT_CC",
            Name = "Tủ Trực Thuốc Cấp Cứu (Khoa Hồi Sức Tích Cực ICU)",
            WarehouseType = WarehouseType.EmergencyCabinet,
            DepartmentName = "Khoa Cấp cứu & Hồi sức tích cực",
            Location = "Tầng 2 - Nhà A",
            ManagerName = "Điều dưỡng trưởng Phạm Lan Anh",
            IsActive = true
        };

        context.Warehouses.AddRange(whMain, whOutpatient, whIcu);
        await context.SaveChangesAsync();

        // 2. Danh mục Thuốc & Vật tư (Medicines)
        var medPara = new Medicine
        {
            Code = "PARA500",
            Name = "Panadol Extra 500mg",
            RegistrationNumber = "VN-18234-14",
            ActiveIngredient = "Paracetamol, Caffeine",
            Concentration = "500mg + 65mg",
            UnitName = "Viên",
            PackageSpecification = "Hộp 15 vỉ x 12 viên",
            RouteOfAdministration = "Đường uống",
            MedicineGroup = "Thuốc giảm đau, hạ sốt, chống viêm không steroid (NSAID)",
            MedicineType = MedicineType.Medicine,
            SpecialControl = SpecialControlType.None,
            InboundPrice = 1200m,
            SalePrice = 1600m,
            InsurancePrice = 1350m,
            MinStockLevel = 500m,
            MaxStockLevel = 20000m
        };

        var medAmox = new Medicine
        {
            Code = "AMOX500",
            Name = "Amoxicillin 500mg Dược Hậu Giang",
            RegistrationNumber = "VD-24512-16",
            ActiveIngredient = "Amoxicillin trihydrat",
            Concentration = "500mg",
            UnitName = "Viên",
            PackageSpecification = "Hộp 10 vỉ x 10 viên nang",
            RouteOfAdministration = "Đường uống",
            MedicineGroup = "Kháng sinh nhóm Penicillin phổ rộng",
            MedicineType = MedicineType.Medicine,
            SpecialControl = SpecialControlType.None,
            InboundPrice = 850m,
            SalePrice = 1200m,
            InsurancePrice = 950m,
            MinStockLevel = 300m,
            MaxStockLevel = 10000m
        };

        var medMorphin = new Medicine
        {
            Code = "MORP10",
            Name = "Morphin hydroclorid 10mg/1ml",
            RegistrationNumber = "VD-19823-13",
            ActiveIngredient = "Morphin hydroclorid",
            Concentration = "10mg/1ml",
            UnitName = "Ống",
            PackageSpecification = "Hộp 10 ống tiêm",
            RouteOfAdministration = "Tiêm bắp, tiêm tĩnh mạch",
            MedicineGroup = "Thuốc giảm đau gây nghiện Opioid",
            MedicineType = MedicineType.Medicine,
            SpecialControl = SpecialControlType.Narcotic, // GÂY NGHIỆN
            InboundPrice = 8500m,
            SalePrice = 12000m,
            InsurancePrice = 9200m,
            MinStockLevel = 50m,
            MaxStockLevel = 500m
        };

        var medDiazepam = new Medicine
        {
            Code = "DIAZ5",
            Name = "Diazepam 5mg Vinphaco",
            RegistrationNumber = "VD-20114-13",
            ActiveIngredient = "Diazepam",
            Concentration = "5mg",
            UnitName = "Viên",
            PackageSpecification = "Hộp 2 vỉ x 10 viên",
            RouteOfAdministration = "Đường uống",
            MedicineGroup = "Thuốc an thần, bình thần nhóm Benzodiazepin",
            MedicineType = MedicineType.Medicine,
            SpecialControl = SpecialControlType.Psychotropic, // HƯỚNG THẦN
            InboundPrice = 1500m,
            SalePrice = 2200m,
            InsurancePrice = 1700m,
            MinStockLevel = 100m,
            MaxStockLevel = 2000m
        };

        var medCefixim = new Medicine
        {
            Code = "CEF200",
            Name = "Cefixim 200mg TV.Pharm",
            RegistrationNumber = "VD-27612-17",
            ActiveIngredient = "Cefixim",
            Concentration = "200mg",
            UnitName = "Viên",
            PackageSpecification = "Hộp 2 vỉ x 10 viên",
            RouteOfAdministration = "Đường uống",
            MedicineGroup = "Kháng sinh nhóm Cephalosporin thế hệ 3",
            MedicineType = MedicineType.Medicine,
            SpecialControl = SpecialControlType.Antibiotic,
            InboundPrice = 3200m,
            SalePrice = 4500m,
            InsurancePrice = 3600m,
            MinStockLevel = 200m,
            MaxStockLevel = 5000m
        };

        var medSyr = new Medicine
        {
            Code = "SYR5ML",
            Name = "Bơm tiêm dùng 1 lần 5ml Vinahankook",
            RegistrationNumber = "SP-0123/BYT-TB-CT",
            ActiveIngredient = "Polypropylene, thép y tế",
            Concentration = "5ml - Kim 23G",
            UnitName = "Cái",
            PackageSpecification = "Hộp 100 cái",
            RouteOfAdministration = "Tiêm truyền",
            MedicineGroup = "Vật tư tiêu hao y tế",
            MedicineType = MedicineType.MedicalSupply,
            SpecialControl = SpecialControlType.None,
            InboundPrice = 650m,
            SalePrice = 1000m,
            InsurancePrice = 750m,
            MinStockLevel = 1000m,
            MaxStockLevel = 30000m
        };

        context.Medicines.AddRange(medPara, medAmox, medMorphin, medDiazepam, medCefixim, medSyr);
        await context.SaveChangesAsync();

        // 3. Các Lô Thuốc (InventoryBatches) - Minh họa FEFO và Cảnh báo
        var now = DateTime.UtcNow;

        // Paracetamol tại Kho lẻ: 2 lô (Lô 1 hạn gần hơn Lô 2)
        var batchPara1 = new InventoryBatch
        {
            WarehouseId = whOutpatient.Id,
            MedicineId = medPara.Id,
            BatchNumber = "L240601",
            ManufactureDate = now.AddMonths(-12),
            ExpiryDate = now.AddDays(45), // CẬN DATE (< 90 ngày)
            InboundPrice = 1200m,
            SalePrice = 1600m,
            QuantityOnHand = 400m,
            QuantityReserved = 0
        };

        var batchPara2 = new InventoryBatch
        {
            WarehouseId = whOutpatient.Id,
            MedicineId = medPara.Id,
            BatchNumber = "L240905",
            ManufactureDate = now.AddMonths(-6),
            ExpiryDate = now.AddMonths(18), // DATE DÀI
            InboundPrice = 1250m,
            SalePrice = 1600m,
            QuantityOnHand = 2500m,
            QuantityReserved = 0
        };

        // Amoxicillin tại Kho chính: 1 lô
        var batchAmoxMain = new InventoryBatch
        {
            WarehouseId = whMain.Id,
            MedicineId = medAmox.Id,
            BatchNumber = "AM24101",
            ManufactureDate = now.AddMonths(-4),
            ExpiryDate = now.AddMonths(20),
            InboundPrice = 850m,
            SalePrice = 1200m,
            QuantityOnHand = 8000m,
            QuantityReserved = 0
        };

        // Amoxicillin tại Kho lẻ: TỒN DƯỚI ĐỊNH MỨC (< 300)
        var batchAmoxSub = new InventoryBatch
        {
            WarehouseId = whOutpatient.Id,
            MedicineId = medAmox.Id,
            BatchNumber = "AM24022",
            ManufactureDate = now.AddMonths(-14),
            ExpiryDate = now.AddMonths(10),
            InboundPrice = 850m,
            SalePrice = 1200m,
            QuantityOnHand = 120m, // DƯỚI MIN STOCK (120 < 300)
            QuantityReserved = 0
        };

        // Morphin tại Tủ trực cấp cứu
        var batchMorphin = new InventoryBatch
        {
            WarehouseId = whIcu.Id,
            MedicineId = medMorphin.Id,
            BatchNumber = "MP2401",
            ManufactureDate = now.AddMonths(-3),
            ExpiryDate = now.AddMonths(24),
            InboundPrice = 8500m,
            SalePrice = 12000m,
            QuantityOnHand = 45m,
            QuantityReserved = 0
        };

        // Diazepam tại Kho lẻ: 1 lô HẾT DATE ĐỂ TEST CẢNH BÁO
        var batchDiazExpired = new InventoryBatch
        {
            WarehouseId = whOutpatient.Id,
            MedicineId = medDiazepam.Id,
            BatchNumber = "DZ2308",
            ManufactureDate = now.AddYears(-3),
            ExpiryDate = now.AddDays(-10), // ĐÃ HẾT HẠN SỬ DỤNG
            InboundPrice = 1500m,
            SalePrice = 2200m,
            QuantityOnHand = 50m,
            QuantityReserved = 0
        };

        context.InventoryBatches.AddRange(batchPara1, batchPara2, batchAmoxMain, batchAmoxSub, batchMorphin, batchDiazExpired);
        await context.SaveChangesAsync();

        // 4. Sổ thẻ kho ban đầu (StockLedger)
        context.StockLedgers.AddRange(
            new StockLedger
            {
                LedgerCode = "TK-2026-0001",
                WarehouseId = whOutpatient.Id,
                MedicineId = medPara.Id,
                BatchId = batchPara1.Id,
                BatchNumber = "L240601",
                ExpiryDate = batchPara1.ExpiryDate,
                TransactionType = LedgerTransactionType.Inbound,
                DocumentNumber = "PN2601-0012",
                Quantity = 500m,
                BalanceBefore = 0,
                BalanceAfter = 500m,
                UnitPrice = 1200m,
                TransactionDate = now.AddDays(-20),
                CreatedBy = "Thủ kho",
                Note = "Nhập kho NCC Dược Phẩm Trung ương 1"
            },
            new StockLedger
            {
                LedgerCode = "TK-2026-0002",
                WarehouseId = whOutpatient.Id,
                MedicineId = medPara.Id,
                BatchId = batchPara1.Id,
                BatchNumber = "L240601",
                ExpiryDate = batchPara1.ExpiryDate,
                TransactionType = LedgerTransactionType.PrescriptionDispense,
                DocumentNumber = "DT2601-8841",
                Quantity = -100m,
                BalanceBefore = 500m,
                BalanceAfter = 400m,
                UnitPrice = 1600m,
                TransactionDate = now.AddDays(-5),
                CreatedBy = "DS. Đặng Minh Quân",
                Note = "Cấp phát đơn thuốc ngoại trú"
            }
        );
        await context.SaveChangesAsync();

        // 5. Đơn thuốc mẫu (PrescriptionOrders)
        var orderPending = new PrescriptionOrder
        {
            PrescriptionCode = "DT-NGOAI-2026001",
            SourceType = PrescriptionSourceType.Outpatient,
            PatientCode = "BN26010001",
            PatientName = "NGUYỄN VĂN AN",
            DoctorName = "BS.CKI. Trần Minh Tuấn",
            Diagnosis = "Viêm họng cấp, sốt nhẹ (J02)",
            PrescribedAt = now.AddHours(-1),
            WarehouseId = whOutpatient.Id,
            Status = PrescriptionStatus.Pending,
            TotalAmount = 32_000m
        };

        orderPending.Details.Add(new PrescriptionOrderDetail
        {
            MedicineId = medPara.Id,
            MedicineCode = medPara.Code,
            MedicineName = medPara.Name,
            UnitName = medPara.UnitName,
            QuantityRequested = 20m,
            QuantityDispensed = 0m,
            UnitPrice = medPara.SalePrice,
            UsageInstructions = "Uống 1 viên khi sốt trên 38.5 độ C, cách nhau 4-6 tiếng"
        });

        context.PrescriptionOrders.Add(orderPending);
        await context.SaveChangesAsync();
    }
}

