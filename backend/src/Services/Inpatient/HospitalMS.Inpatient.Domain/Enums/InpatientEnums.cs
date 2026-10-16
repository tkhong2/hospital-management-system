namespace HospitalMS.Inpatient.Domain.Enums;

public enum BedStatus
{
    Empty = 0,       // Giường trống
    Occupied = 1,    // Đang có bệnh nhân
    Cleaning = 2,    // Đang dọn vệ sinh / Khử trùng
    Maintenance = 3  // Hỏng / Đang bảo trì
}

public enum BedType
{
    Standard = 0,       // Giường thường nội khoa/ngoại khoa
    Emergency = 1,      // Giường hồi sức / cấp cứu (ICU)
    RequestService = 2  // Giường dịch vụ theo yêu cầu / Phòng đơn
}

public enum AdmissionStatus
{
    Admitted = 0,     // Đang nằm viện điều trị
    Transferring = 1, // Đang chuyển khoa
    Discharged = 2,   // Đã xuất viện / Tổng kết bệnh án
    Cancelled = 3     // Hủy bệnh án
}

public enum DischargeOutcome
{
    Cured = 0,     // Khỏi bệnh
    Improved = 1,  // Đỡ / Giảm
    Unchanged = 2, // Không thay đổi
    Worsened = 3,  // Nặng hơn
    Deceased = 4   // Tử vong
}

public enum CareLevel
{
    Level1 = 1, // Chăm sóc cấp 1: Bệnh nhân rất nặng, bất động, hôn mê
    Level2 = 2, // Chăm sóc cấp 2: Bệnh nhân nặng, vận động khó khăn
    Level3 = 3  // Chăm sóc cấp 3: Bệnh nhân nhẹ, có thể tự phục vụ
}

