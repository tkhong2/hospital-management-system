namespace HospitalMS.HealthExam.Domain.Enums;

public enum ContractStatus
{
    Draft = 1,
    Active = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}

public enum ExamLocationType
{
    AtHospital = 1,   // Khám tại cơ sở y tế / viện
    OnSite = 2        // Khám lưu động tại cơ quan / nhà máy / công ty
}

public enum PaymentMethod
{
    BankTransfer = 1, // Chuyển khoản ngân hàng
    Cash = 2,         // Tiền mặt
    CreditCard = 3    // Thẻ thanh toán / POS
}

public enum TargetGender
{
    All = 1,
    Male = 2,
    Female = 3,
    MarriedFemale = 4, // Nữ có gia đình (khám sản chuyên biệt)
    SingleFemale = 5   // Nữ độc thân
}

public enum ExamType
{
    Corporate = 1,         // Khám định kỳ theo hợp đồng công ty/đoàn
    Driver = 2,            // Khám sức khỏe lái xe (TT 24/2015)
    PreEmployment = 3,     // Khám tuyển dụng / xin việc
    StudyAbroad = 4,       // Khám du học / xuất khẩu lao động
    PeriodicIndividual = 5 // Khám cá nhân định kỳ
}

/// <summary>
/// Trạng thái hồ sơ người khám theo vòng đời khép kín
/// </summary>
public enum RegistrationStatus
{
    Pending = 1,             // 1. Chờ khám (mới import / đăng ký)
    CheckedIn = 2,           // 2. Đã tiếp đón / check-in / cấp STT
    InProgress = 3,          // 3. Đang khám tại các trạm chuyên khoa lâm sàng
    WaitingForLab = 4,       // 4. Chờ kết quả Cận lâm sàng (Xét nghiệm, CĐHA)
    WaitingForConclusion = 5,// 5. Đã đủ KQ lâm sàng & CLS -> Chờ Bác sĩ kết luận
    Completed = 6,           // 6. Bác sĩ đã kết luận & ký duyệt hoàn tất
    Delivered = 7,           // 7. Đã trả kết quả (bản giấy / PDF / ký số)
    Cancelled = 8            // Hủy hồ sơ khám
}

public enum VisitStatus
{
    CheckedIn = 1,
    InProgress = 2,
    WaitingForLab = 3,
    WaitingForConclusion = 4,
    Completed = 5,
    Cancelled = 6
}

public enum StationStatus
{
    Waiting = 1,   // Đang chờ đến lượt
    Examining = 2, // Đang được khám
    Completed = 3, // Đã có kết quả khám chuyên khoa
    Skipped = 4    // Bỏ qua (không bắt buộc hoặc bệnh nhân từ chối)
}

public enum LabOrderType
{
    Laboratory = 1,      // Xét nghiệm (Huyết học, Sinh hóa, Nước tiểu...)
    Imaging = 2,         // Chẩn đoán hình ảnh (X-quang, Siêu âm...)
    FunctionalProbe = 3  // Thăm dò chức năng (Điện tim, Đo chức năng hô hấp...)
}

public enum LabOrderStatus
{
    Pending = 1,    // Mới chỉ định
    Sampling = 2,   // Đã lấy mẫu bệnh phẩm / chụp
    Processing = 3, // Đang chạy máy xét nghiệm / đọc phim
    Completed = 4,  // Đã có kết quả
    Cancelled = 5   // Hủy chỉ định
}

public enum AbnormalFlag
{
    Normal = 0,        // Bình thường trong khoảng tham chiếu
    Low = 1,           // Thấp hơn giới hạn dưới
    High = 2,          // Cao hơn giới hạn trên
    CriticalLow = 3,   // Nguy kịch thấp
    CriticalHigh = 4,  // Nguy kịch cao
    Positive = 5       // Dương tính bất thường (với chỉ số định tính)
}

/// <summary>
/// Phân loại sức khỏe theo Thông tư 14/2013/TT-BYT & Thông tư 24/2015/TTLT-BYT-BGTVT
/// </summary>
public enum HealthClassification
{
    Unclassified = 0,
    Type1 = 1,   // Loại I: Rất khỏe
    Type2 = 2,   // Loại II: Khỏe
    Type3 = 3,   // Loại III: Trung bình
    Type4 = 4,   // Loại IV: Yếu
    Type5 = 5,   // Loại V: Rất yếu
    Qualified = 10,   // Đạt tiêu chuẩn sức khỏe lái xe (TT 24/2015)
    Unqualified = 11  // Không đạt tiêu chuẩn sức khỏe lái xe
}

public enum CertificateType
{
    GeneralTT14 = 1,   // Giấy KSK theo Thông tư 14/2013/TT-BYT
    DriverTT24 = 2,    // Giấy KSK người lái xe theo TT 24/2015/TTLT-BYT-BGTVT
    StudentAbroad = 3, // Giấy KSK du học / lao động nước ngoài
    FoodHandler = 4    // Giấy chứng nhận sức khỏe cho người trực tiếp chế biến thực phẩm
}

public enum DeliveryMethod
{
    AtCounter = 1,    // Nhận trực tiếp tại quầy trả kết quả
    CompanyBatch = 2, // Trả cả đoàn theo hợp đồng doanh nghiệp
    Email = 3,        // Gửi tệp PDF qua Email tự động
    Zalo = 4          // Gửi thông báo qua Zalo ZNS
}

