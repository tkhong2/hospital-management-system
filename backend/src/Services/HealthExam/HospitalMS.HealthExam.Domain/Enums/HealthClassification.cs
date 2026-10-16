namespace HospitalMS.HealthExam.Domain.Enums;

/// <summary>
/// Phân loại sức khỏe theo Thông tư 14/2013/TT-BYT và Thông tư 24/2015/TTLT-BYT-BGTVT
/// </summary>
public enum HealthClassification
{
    Unclassified = 0,
    Type1 = 1, // Loại I: Rất khỏe
    Type2 = 2, // Loại II: Khỏe
    Type3 = 3, // Loại III: Trung bình
    Type4 = 4, // Loại IV: Yếu
    Type5 = 5, // Loại V: Rất yếu
    Qualified = 10,   // Đạt tiêu chuẩn (Khám lái xe TT24)
    Unqualified = 11  // Không đạt tiêu chuẩn (Khám lái xe TT24)
}

public enum ExamType
{
    Corporate = 1,   // Khám sức khỏe định kỳ theo đoàn/hợp đồng công ty
    Driver = 2,      // Khám sức khỏe lái xe (TT 24/2015)
    PreEmployment = 3,// Khám sức khỏe tuyển dụng / xin việc
    StudyAbroad = 4, // Khám du học / xuất khẩu lao động
    PeriodicIndividual = 5 // Khám cá nhân định kỳ
}

public enum RecordStatus
{
    Pending = 1,           // Chờ khám
    InProgress = 2,        // Đang khám tại các trạm
    WaitingForLab = 3,     // Chờ kết quả Cận lâm sàng (Xét nghiệm / CĐHA)
    WaitingForConclusion = 4, // Chờ bác sĩ trưởng đoàn kết luận
    Completed = 5,         // Hoàn tất kết luận
    Delivered = 6          // Đã trả kết quả (bản giấy / PDF / gửi điện tử)
}
