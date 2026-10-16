namespace HospitalMS.Pharmacy.Domain.Enums;

public enum MedicineType
{
    Medicine = 1,       // Thuốc tân dược / đông y
    MedicalSupply = 2,  // Vật tư y tế tiêu hao (bơm kim tiêm, bông băng, găng tay...)
    Chemical = 3        // Hóa chất xét nghiệm / sinh phẩm chẩn đoán
}

public enum SpecialControlType
{
    None = 0,         // Thuốc thường
    Narcotic = 1,     // Thuốc gây nghiện (Morphin, Fentanyl...)
    Psychotropic = 2, // Thuốc hướng thần (Diazepam, Phenobarbital...)
    Precursor = 3,    // Thuốc tiền chất (Ephedrine, Pseudoephedrine...)
    Toxic = 4,        // Thuốc độc bảng A/B
    Antibiotic = 5    // Kháng sinh ưu tiên quản lý / hạn chế
}

public enum WarehouseType
{
    MainWarehouse = 1,    // Kho chẵn / kho trung tâm (kho tổng)
    SubWarehouse = 2,     // Kho lẻ phát thuốc ngoại trú
    EmergencyCabinet = 3, // Tủ trực cấp cứu tại khoa lâm sàng
    InpatientWarehouse = 4// Kho dược nội trú
}

public enum DocumentType
{
    Inbound = 1,  // Nhập kho từ nhà cung cấp
    Outbound = 2, // Xuất kho (sử dụng / viện trợ...)
    Transfer = 3, // Chuyển kho nội bộ (Kho chính -> Kho lẻ / Tủ trực)
    Return = 4,   // Trả hàng (Kho lẻ trả kho chính, hoặc trả nhà cung cấp)
    Dispose = 5   // Hủy thuốc (hỏng, vỡ, hết hạn dùng)
}

public enum DocumentStatus
{
    Draft = 1,    // Bản nháp, đang lập phiếu
    Approved = 2, // Đã duyệt (ghi thẻ kho và cập nhật tồn thực tế)
    Cancelled = 3 // Đã hủy
}

public enum LedgerTransactionType
{
    Inbound = 1,              // Nhập kho NCC (+)
    Outbound = 2,             // Xuất kho (-)
    TransferIn = 3,           // Nhận chuyển kho (+)
    TransferOut = 4,          // Xuất chuyển kho (-)
    Return = 5,               // Trả lại kho / trả NCC (+/-)
    Dispose = 6,              // Xuất hủy (-)
    InventoryAdjustment = 7,  // Cân bằng kiểm kê (+/-)
    PrescriptionDispense = 8  // Cấp phát đơn thuốc cho bệnh nhân (-)
}

public enum PrescriptionSourceType
{
    Outpatient = 1, // Đơn thuốc khám ngoại trú
    Inpatient = 2,  // Y lệnh thuốc nội trú
    HealthExam = 3  // Đơn thuốc tư vấn khám sức khỏe
}

public enum PrescriptionStatus
{
    Pending = 1,   // Chờ duyệt / chờ lấy thuốc
    Approved = 2,  // Đã duyệt & giữ chỗ (Reserved) theo FEFO
    Dispensed = 3, // Đã cấp phát thực tế cho người bệnh (trừ kho & ghi thẻ kho)
    Cancelled = 4  // Hủy đơn / trả lại
}

