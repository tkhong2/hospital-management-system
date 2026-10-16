namespace HospitalMS.Catalog.Domain.Enums;

public enum ServiceType
{
    Examination = 1,       // Khám bệnh
    Laboratory = 2,        // Cận lâm sàng / Xét nghiệm
    Imaging = 3,           // Chẩn đoán hình ảnh (X-Quang, Siêu âm, CT, MRI)
    FunctionalProbe = 4,   // Thăm dò chức năng (Điện tim, Đo chức năng hô hấp)
    Procedure = 5          // Thủ thuật / Phẫu thuật
}

