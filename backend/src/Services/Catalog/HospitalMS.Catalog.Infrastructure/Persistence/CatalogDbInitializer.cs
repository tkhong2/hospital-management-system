using HospitalMS.Catalog.Domain.Entities;
using HospitalMS.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Catalog.Infrastructure.Persistence;

public class CatalogDbInitializer(
    CatalogDbContext dbContext,
    ILogger<CatalogDbInitializer> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            if (dbContext.Database.IsRelational())
            {
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            await SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo dữ liệu mẫu CatalogService.");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        // 1. Seed Units
        if (!await dbContext.Units.AnyAsync())
        {
            var units = new List<Unit>
            {
                new() { Code = "LAN", Name = "Lần", Description = "Lượt khám / lần chỉ định" },
                new() { Code = "CA", Name = "Ca", Description = "Ca phẫu thuật / thủ thuật" },
                new() { Code = "XN", Name = "Xét nghiệm", Description = "Chỉ số xét nghiệm" },
                new() { Code = "VIEN", Name = "Viên", Description = "Viên thuốc" },
                new() { Code = "GOI", Name = "Gói", Description = "Gói thuốc / vật tư" },
                new() { Code = "LO", Name = "Lọ / Chai", Description = "Lọ dung dịch tiêm / truyền" }
            };
            dbContext.Units.AddRange(units);
            await dbContext.SaveChangesAsync();
        }

        // 2. Seed Specialties
        if (!await dbContext.Specialties.AnyAsync())
        {
            var specialties = new List<Specialty>
            {
                new() { Code = "NOI", Name = "Nội khoa", Description = "Khám và điều trị bệnh nội khoa tổng quát" },
                new() { Code = "NGOAI", Name = "Ngoại khoa", Description = "Khám ngoại, tiểu phẫu, đại phẫu" },
                new() { Code = "MAT", Name = "Mắt", Description = "Nhãn khoa, kiểm tra thị lực, khúc xạ" },
                new() { Code = "TMH", Name = "Tai Mũi Họng", Description = "Nội soi TMH, tầm soát bệnh đường hô hấp trên" },
                new() { Code = "RHM", Name = "Răng Hàm Mặt", Description = "Nha khoa tổng quát, cạo vôi, nhổ răng" },
                new() { Code = "DL", Name = "Da liễu", Description = "Bệnh lý ngoài da, thẩm mỹ da liễu" },
                new() { Code = "SAN", Name = "Sản Phụ Khoa", Description = "Khám phụ khoa, tầm soát ung thư CTC, quản lý thai kỳ" },
                new() { Code = "NHI", Name = "Nhi khoa", Description = "Khám và chăm sóc sức khỏe trẻ em" }
            };
            dbContext.Specialties.AddRange(specialties);
            await dbContext.SaveChangesAsync();
        }

        // 3. Seed Departments
        if (!await dbContext.Departments.AnyAsync())
        {
            var depts = new List<Department>
            {
                new() { Code = "KKB", Name = "Khoa Khám Bệnh", Type = "Clinical", Location = "Tầng 1 - Tòa nhà A", PhoneNumber = "024.3825.1001" },
                new() { Code = "KNOI", Name = "Khoa Nội Tổng Hợp", Type = "Clinical", Location = "Tầng 3 - Tòa nhà A", PhoneNumber = "024.3825.1002" },
                new() { Code = "KNGOAI", Name = "Khoa Ngoại Tổng Quát", Type = "Clinical", Location = "Tầng 4 - Tòa nhà A", PhoneNumber = "024.3825.1003" },
                new() { Code = "CDHA", Name = "Khoa Chẩn Đoán Hình Ảnh", Type = "Paraclinical", Location = "Tầng 1 - Tòa nhà B", PhoneNumber = "024.3825.1004" },
                new() { Code = "KXN", Name = "Khoa Xét Nghiệm", Type = "Paraclinical", Location = "Tầng 2 - Tòa nhà B", PhoneNumber = "024.3825.1005" },
                new() { Code = "KD", Name = "Khoa Dược", Type = "Paraclinical", Location = "Tầng 1 - Tòa nhà C", PhoneNumber = "024.3825.1006" }
            };
            dbContext.Departments.AddRange(depts);
            await dbContext.SaveChangesAsync();
        }

        // 4. Seed ICD-10
        if (!await dbContext.Icd10s.AnyAsync())
        {
            var icds = new List<Icd10>
            {
                new() { Code = "J00", Name = "Viêm mũi họng cấp [cảm thường]", NameEn = "Acute nasopharyngitis [common cold]", ChapterCode = "X", ChapterName = "Bệnh hệ hô hấp" },
                new() { Code = "J02", Name = "Viêm họng cấp", NameEn = "Acute pharyngitis", ChapterCode = "X", ChapterName = "Bệnh hệ hô hấp" },
                new() { Code = "J06", Name = "Nhiễm khuẩn đường hô hấp trên cấp ở nhiều vị trí và không xác định", NameEn = "Acute upper respiratory infections", ChapterCode = "X", ChapterName = "Bệnh hệ hô hấp" },
                new() { Code = "J20", Name = "Viêm phế quản cấp", NameEn = "Acute bronchitis", ChapterCode = "X", ChapterName = "Bệnh hệ hô hấp" },
                new() { Code = "I10", Name = "Bệnh tăng huyết áp vô căn (nguyên phát)", NameEn = "Essential (primary) hypertension", ChapterCode = "IX", ChapterName = "Bệnh hệ tuần hoàn" },
                new() { Code = "E11", Name = "Bệnh đái tháo đường không phụ thuộc insulin (typ 2)", NameEn = "Non-insulin-dependent diabetes mellitus", ChapterCode = "IV", ChapterName = "Bệnh nội tiết, dinh dưỡng và chuyển hóa" },
                new() { Code = "K29", Name = "Viêm dạ dày và tá tràng", NameEn = "Gastritis and duodenitis", ChapterCode = "XI", ChapterName = "Bệnh hệ tiêu hóa" },
                new() { Code = "M54.5", Name = "Đau lưng dưới", NameEn = "Low back pain", ChapterCode = "XIII", ChapterName = "Bệnh hệ cơ xương khớp và mô liên kết" },
                new() { Code = "H52.1", Name = "Cận thị", NameEn = "Myopia", ChapterCode = "VII", ChapterName = "Bệnh mắt và phần phụ" },
                new() { Code = "Z00.0", Name = "Khám kiểm tra sức khỏe tổng quát", NameEn = "General medical examination", ChapterCode = "XXI", ChapterName = "Các yếu tố ảnh hưởng sức khỏe" }
            };
            dbContext.Icd10s.AddRange(icds);
            await dbContext.SaveChangesAsync();
        }

        // 5. Seed Medical Services
        if (!await dbContext.MedicalServices.AnyAsync())
        {
            var services = new List<MedicalService>
            {
                // Khám
                new() { Code = "KB01", Name = "Khám Nội tổng quát", ServiceType = ServiceType.Examination, UnitName = "Lần", Price = 150000, HealthInsurancePrice = 39000, DepartmentName = "Khoa Khám Bệnh" },
                new() { Code = "KB02", Name = "Khám Chuyên khoa Mắt", ServiceType = ServiceType.Examination, UnitName = "Lần", Price = 120000, HealthInsurancePrice = 39000, DepartmentName = "Khoa Khám Bệnh" },
                new() { Code = "KB03", Name = "Khám Chuyên khoa Tai Mũi Họng", ServiceType = ServiceType.Examination, UnitName = "Lần", Price = 120000, HealthInsurancePrice = 39000, DepartmentName = "Khoa Khám Bệnh" },
                new() { Code = "KB04", Name = "Khám Chuyên khoa Răng Hàm Mặt", ServiceType = ServiceType.Examination, UnitName = "Lần", Price = 120000, HealthInsurancePrice = 39000, DepartmentName = "Khoa Khám Bệnh" },
                new() { Code = "KB05", Name = "Khám Phụ khoa", ServiceType = ServiceType.Examination, UnitName = "Lần", Price = 150000, HealthInsurancePrice = 39000, DepartmentName = "Khoa Khám Bệnh" },

                // Cận lâm sàng / Xét nghiệm
                new() { Code = "XN01", Name = "Tổng phân tích tế bào máu ngoại vi (bằng máy đếm laser - 18 thông số)", ServiceType = ServiceType.Laboratory, UnitName = "Xét nghiệm", Price = 80000, HealthInsurancePrice = 45000, DepartmentName = "Khoa Xét Nghiệm" },
                new() { Code = "XN02", Name = "Định lượng Glucose máu", ServiceType = ServiceType.Laboratory, UnitName = "Xét nghiệm", Price = 45000, HealthInsurancePrice = 25000, DepartmentName = "Khoa Xét Nghiệm" },
                new() { Code = "XN03", Name = "Định lượng Cholesterol toàn phần", ServiceType = ServiceType.Laboratory, UnitName = "Xét nghiệm", Price = 50000, HealthInsurancePrice = 28000, DepartmentName = "Khoa Xét Nghiệm" },
                new() { Code = "XN04", Name = "Đo hoạt độ AST (GOT)", ServiceType = ServiceType.Laboratory, UnitName = "Xét nghiệm", Price = 45000, HealthInsurancePrice = 26000, DepartmentName = "Khoa Xét Nghiệm" },
                new() { Code = "XN05", Name = "Đo hoạt độ ALT (GPT)", ServiceType = ServiceType.Laboratory, UnitName = "Xét nghiệm", Price = 45000, HealthInsurancePrice = 26000, DepartmentName = "Khoa Xét Nghiệm" },
                new() { Code = "XN06", Name = "Tổng phân tích nước tiểu (bằng máy tự động 10 thông số)", ServiceType = ServiceType.Laboratory, UnitName = "Xét nghiệm", Price = 55000, HealthInsurancePrice = 30000, DepartmentName = "Khoa Xét Nghiệm" },

                // CĐHA & Thăm dò chức năng
                new() { Code = "HA01", Name = "Chụp X-quang tim phổi thẳng", ServiceType = ServiceType.Imaging, UnitName = "Lần", Price = 120000, HealthInsurancePrice = 65000, DepartmentName = "Khoa Chẩn Đoán Hình Ảnh" },
                new() { Code = "HA02", Name = "Siêu âm ổ bụng tổng quát (màu)", ServiceType = ServiceType.Imaging, UnitName = "Lần", Price = 180000, HealthInsurancePrice = 85000, DepartmentName = "Khoa Chẩn Đoán Hình Ảnh" },
                new() { Code = "TD01", Name = "Ghi điện tim (ECG 12 chuyển đạo)", ServiceType = ServiceType.FunctionalProbe, UnitName = "Lần", Price = 70000, HealthInsurancePrice = 40000, DepartmentName = "Khoa Chẩn Đoán Hình Ảnh" },
                new() { Code = "TD02", Name = "Đo chức năng hô hấp", ServiceType = ServiceType.FunctionalProbe, UnitName = "Lần", Price = 150000, HealthInsurancePrice = 90000, DepartmentName = "Khoa Chẩn Đoán Hình Ảnh" },

                // Thủ thuật
                new() { Code = "TT01", Name = "Nội soi tai mũi họng dải tần hẹp (NBI)", ServiceType = ServiceType.Procedure, UnitName = "Lần", Price = 250000, HealthInsurancePrice = 160000, DepartmentName = "Khoa Khám Bệnh" },
                new() { Code = "TT02", Name = "Lấy cao răng và đánh bóng hai hàm", ServiceType = ServiceType.Procedure, UnitName = "Lần", Price = 200000, HealthInsurancePrice = 0, DepartmentName = "Khoa Khám Bệnh" }
            };

            dbContext.MedicalServices.AddRange(services);
            await dbContext.SaveChangesAsync();
        }

        // 6. Seed Administrative Divisions
        if (!await dbContext.AdministrativeDivisions.AnyAsync())
        {
            var divisions = new List<AdministrativeDivision>
            {
                // Provinces
                new() { Code = "01", Name = "Thành phố Hà Nội", Level = "Province", ParentCode = null },
                new() { Code = "79", Name = "Thành phố Hồ Chí Minh", Level = "Province", ParentCode = null },
                new() { Code = "48", Name = "Thành phố Đà Nẵng", Level = "Province", ParentCode = null },

                // Districts
                new() { Code = "001", Name = "Quận Cầu Giấy", Level = "District", ParentCode = "01" },
                new() { Code = "002", Name = "Quận Đống Đa", Level = "District", ParentCode = "01" },
                new() { Code = "760", Name = "Quận 1", Level = "District", ParentCode = "79" },
                new() { Code = "769", Name = "Thành phố Thủ Đức", Level = "District", ParentCode = "79" },
                new() { Code = "490", Name = "Quận Hải Châu", Level = "District", ParentCode = "48" },

                // Wards
                new() { Code = "0001", Name = "Phường Nghĩa Đô", Level = "Ward", ParentCode = "001" },
                new() { Code = "0002", Name = "Phường Dịch Vọng", Level = "Ward", ParentCode = "001" },
                new() { Code = "7601", Name = "Phường Bến Nghé", Level = "Ward", ParentCode = "760" },
                new() { Code = "7602", Name = "Phường Đa Kao", Level = "Ward", ParentCode = "760" },
                new() { Code = "4901", Name = "Phường Nam Dương", Level = "Ward", ParentCode = "490" }
            };

            dbContext.AdministrativeDivisions.AddRange(divisions);
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation("Đã khởi tạo thành công toàn bộ danh mục dùng chung (CatalogService).");
    }
}

