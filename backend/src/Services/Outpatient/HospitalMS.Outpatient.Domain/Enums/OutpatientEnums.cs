namespace HospitalMS.Outpatient.Domain.Enums;

public enum ReceptionStatus
{
    Waiting = 0,         // Chờ gọi khám
    InConsultation = 1,  // Đang khám
    WaitingForLab = 2,   // Chờ kết quả CLS
    Completed = 3,       // Hoàn tất lượt khám
    Cancelled = 4        // Đã hủy lượt khám
}

public enum PatientType
{
    OutpatientInsurance = 0, // Khám BHYT
    ServiceFee = 1           // Khám Viện phí / Yêu cầu
}

public enum ServiceOrderStatus
{
    Ordered = 0,         // Đã chỉ định
    SampleCollected = 1, // Đã lấy mẫu / tiếp nhận
    Processing = 2,      // Đang thực hiện
    Completed = 3,       // Đã có kết quả
    Cancelled = 4        // Hủy chỉ định
}

public enum PrescriptionStatus
{
    Draft = 0,       // Bản thảo
    Prescribed = 1,  // Đã ký kê đơn
    Dispensed = 2,   // Đã cấp phát thuốc
    Cancelled = 3    // Hủy đơn
}

public enum BillingStatus
{
    Pending = 0,           // Chờ thanh toán
    Paid = 1,              // Đã thanh toán
    PartiallyRefunded = 2, // Đã hoàn tiền một phần
    Cancelled = 3          // Hủy hóa đơn
}

public enum TreatmentDisposition
{
    DischargedWithPrescription = 0, // Kê đơn cho về điều trị ngoại trú
    Hospitalize = 1,                // Chỉ định vào viện điều trị nội trú
    TransferHospital = 2,           // Chuyển viện tuyến trên
    FollowUp = 3                    // Hẹn khám lại
}

