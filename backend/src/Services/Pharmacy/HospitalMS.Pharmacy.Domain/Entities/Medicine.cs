using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Domain.Enums;

namespace HospitalMS.Pharmacy.Domain.Entities;

public class Medicine : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // Mã thuốc (VD: PARA500, AMOX500)
    public string Name { get; set; } = string.Empty; // Tên biệt dược / Tên thương mại
    public string? RegistrationNumber { get; set; } // Số đăng ký lưu hành (SĐK)
    public string ActiveIngredient { get; set; } = string.Empty; // Hoạt chất (Paracetamol)
    public string Concentration { get; set; } = string.Empty; // Hàm lượng (500mg, 1g, 250mg/5ml)
    public string UnitName { get; set; } = "Viên"; // Đơn vị tính cơ sở (Viên, Lọ, Ống, Gói)
    public string? PackageSpecification { get; set; } // Quy cách đóng gói (Hộp 10 vỉ x 10 viên)
    public string? RouteOfAdministration { get; set; } // Đường dùng (Uống, Tiêm, Bôi...)
    public string? MedicineGroup { get; set; } // Nhóm tác dụng dược lý
    public MedicineType MedicineType { get; set; } = MedicineType.Medicine;
    public SpecialControlType SpecialControl { get; set; } = SpecialControlType.None;
    public decimal InboundPrice { get; set; } // Giá nhập gần nhất
    public decimal SalePrice { get; set; } // Giá bán viện phí
    public decimal? InsurancePrice { get; set; } // Giá BHYT thanh toán
    public decimal MinStockLevel { get; set; } = 50; // Mức tồn tối thiểu để cảnh báo đặt hàng
    public decimal MaxStockLevel { get; set; } = 5000; // Mức tồn tối đa
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<InventoryBatch> Batches { get; set; } = new List<InventoryBatch>();
    public ICollection<StockLedger> Ledgers { get; set; } = new List<StockLedger>();
}

