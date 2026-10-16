using HospitalMS.Patient.Domain.Entities;
using HospitalMS.Patient.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Patient.Infrastructure.Persistence;

public class PatientDbInitializer(
    PatientDbContext dbContext,
    ILogger<PatientDbInitializer> logger)
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
            logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo dữ liệu mẫu PatientService.");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        if (await dbContext.Patients.AnyAsync()) return;

        var patients = new List<Domain.Entities.Patient>
        {
            new()
            {
                PatientCode = "BN241000001",
                FullName = "Nguyễn Văn An",
                DateOfBirth = new DateTime(1990, 5, 14, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Male,
                IdentityCardNumber = "001090012345",
                HealthInsuranceNumber = "DN4010120894567",
                PhoneNumber = "0912345678",
                Email = "an.nguyen90@gmail.com",
                DetailAddress = "Số 15 ngõ 120 Hoàng Quốc Việt",
                WardCode = "0001",
                WardName = "Phường Nghĩa Đô",
                DistrictCode = "001",
                DistrictName = "Quận Cầu Giấy",
                ProvinceCode = "01",
                ProvinceName = "Thành phố Hà Nội",
                Ethnic = "Kinh",
                Nationality = "Việt Nam",
                Career = "Kỹ sư công nghệ thông tin",
                BloodType = BloodType.O_Positive,
                Allergies = "Dị ứng Penicillin",
                Note = "Tiền sử hen phế quản nhẹ"
            },
            new()
            {
                PatientCode = "BN241000002",
                FullName = "Trần Thị Bình",
                DateOfBirth = new DateTime(1995, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Female,
                IdentityCardNumber = "079195009876",
                HealthInsuranceNumber = "GD4790123456789",
                PhoneNumber = "0987654321",
                Email = "binh.tran@hospital.vn",
                DetailAddress = "45 Lê Duẩn",
                WardCode = "0002",
                WardName = "Phường Bến Nghé",
                DistrictCode = "002",
                DistrictName = "Quận 1",
                ProvinceCode = "79",
                ProvinceName = "Thành phố Hồ Chí Minh",
                Ethnic = "Kinh",
                Nationality = "Việt Nam",
                Career = "Nhân viên văn phòng",
                BloodType = BloodType.A_Positive,
                Allergies = "Không ghi nhận",
                Note = null
            },
            new()
            {
                PatientCode = "BN241000003",
                FullName = "Lê Hoàng Cường",
                DateOfBirth = new DateTime(1985, 11, 3, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Male,
                IdentityCardNumber = "048085005555",
                HealthInsuranceNumber = "TE1480123456780",
                PhoneNumber = "0905123456",
                Email = "cuong.le@gmail.com",
                DetailAddress = "128 Nguyễn Văn Linh",
                WardCode = "0003",
                WardName = "Phường Nam Dương",
                DistrictCode = "003",
                DistrictName = "Quận Hải Châu",
                ProvinceCode = "48",
                ProvinceName = "Thành phố Đà Nẵng",
                Ethnic = "Kinh",
                Nationality = "Việt Nam",
                Career = "Kinh doanh tự do",
                BloodType = BloodType.B_Positive,
                Allergies = "Dị ứng tôm cua",
                Note = "Tăng huyết áp vô căn đang điều trị"
            },
            new()
            {
                PatientCode = "BN241000004",
                FullName = "Phạm Minh Đức",
                DateOfBirth = new DateTime(1992, 3, 19, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Male,
                IdentityCardNumber = "031092004444",
                HealthInsuranceNumber = "HS4310123456781",
                PhoneNumber = "0936112233",
                Email = "duc.pham@yahoo.com",
                DetailAddress = "88 Lạch Tray",
                WardCode = "0004",
                WardName = "Phường Lạch Tray",
                DistrictCode = "004",
                DistrictName = "Quận Ngô Quyền",
                ProvinceCode = "31",
                ProvinceName = "Thành phố Hải Phòng",
                Ethnic = "Kinh",
                Nationality = "Việt Nam",
                Career = "Giáo viên",
                BloodType = BloodType.AB_Positive,
                Allergies = "Không có",
                Note = null
            },
            new()
            {
                PatientCode = "BN241000005",
                FullName = "Hoàng Thị Em",
                DateOfBirth = new DateTime(1999, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Female,
                IdentityCardNumber = "025199003333",
                HealthInsuranceNumber = "CH4250123456782",
                PhoneNumber = "0948556677",
                Email = "em.hoang@outlook.com",
                DetailAddress = "12 Hòa Bình",
                WardCode = "0005",
                WardName = "Phường Tân An",
                DistrictCode = "005",
                DistrictName = "Quận Ninh Kiều",
                ProvinceCode = "92",
                ProvinceName = "Thành phố Cần Thơ",
                Ethnic = "Kinh",
                Nationality = "Việt Nam",
                Career = "Sinh viên y khoa",
                BloodType = BloodType.O_Positive,
                Allergies = "Dị ứng phấn hoa",
                Note = null
            }
        };

        dbContext.Patients.AddRange(patients);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Đã khởi tạo thành công {Count} hồ sơ bệnh nhân mẫu.", patients.Count);
    }
}

