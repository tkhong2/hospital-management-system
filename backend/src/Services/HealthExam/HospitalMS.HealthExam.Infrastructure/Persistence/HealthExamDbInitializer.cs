using HospitalMS.HealthExam.Domain.Entities;
using HospitalMS.HealthExam.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.HealthExam.Infrastructure.Persistence;

public static class HealthExamDbInitializer
{
    public static async Task SeedAsync(HealthExamDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Companies.AnyAsync())
        {
            return; // Đã có dữ liệu seed
        }

        // 1. Trạm khám chuyên khoa (ExamStations)
        var stationPhysical = new ExamStation
        {
            Code = "ST_PHYSICAL",
            Name = "Trạm Thể lực & Dấu sinh tồn",
            SpecialtyCode = "THE_LUC",
            RoomNumber = "P.101",
            DefaultDoctorName = "ĐD. Nguyễn Thu Trang",
            OrderIndex = 1,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "heightCm", "label": "Chiều cao (cm)", "type": "number", "required": true },
                { "name": "weightKg", "label": "Cân nặng (kg)", "type": "number", "required": true },
                { "name": "bmi", "label": "Chỉ số BMI", "type": "computed", "formula": "weight / ((height/100)^2)" },
                { "name": "pulse", "label": "Mạch (lần/phút)", "type": "number", "required": true },
                { "name": "bpSystolic", "label": "Huyết áp tâm thu (mmHg)", "type": "number", "required": true },
                { "name": "bpDiastolic", "label": "Huyết áp tâm trương (mmHg)", "type": "number", "required": true }
              ]
            }
            """
        };

        var stationInternal = new ExamStation
        {
            Code = "ST_INTERNAL",
            Name = "Trạm Khám Nội tổng quát",
            SpecialtyCode = "NOI",
            RoomNumber = "P.102",
            DefaultDoctorName = "BS.CKI. Trần Minh Tuấn",
            OrderIndex = 2,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "cardiovascular", "label": "Tuần hoàn / Tim mạch", "type": "text", "default": "Tiếng tim T1, T2 rõ, đều, không có tiếng thổi bệnh lý" },
                { "name": "respiratory", "label": "Hô hấp / Phổi", "type": "text", "default": "Rì rào phế nang êm dịu 2 phế trường, không rale" },
                { "name": "digestive", "label": "Tiêu hóa / Bụng", "type": "text", "default": "Bụng mềm, không trướng, gan lách không sờ chạm" },
                { "name": "urinary", "label": "Thận - Tiết niệu", "type": "text", "default": "Chạm thận (-), bập bềnh thận (-)" },
                { "name": "neurological", "label": "Thần kinh - Tâm thần", "type": "text", "default": "Ý thức tỉnh táo, tiếp xúc tốt, phản xạ gân xương bình thường" }
              ]
            }
            """
        };

        var stationEye = new ExamStation
        {
            Code = "ST_EYE",
            Name = "Trạm Khám Mắt",
            SpecialtyCode = "MAT",
            RoomNumber = "P.103",
            DefaultDoctorName = "BS.CKI. Hoàng Mai Lan",
            OrderIndex = 3,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "visionWithoutGlassesLeft", "label": "Thị lực không kính (Trái)", "type": "text", "default": "10/10" },
                { "name": "visionWithoutGlassesRight", "label": "Thị lực không kính (Phải)", "type": "text", "default": "10/10" },
                { "name": "visionWithGlassesLeft", "label": "Thị lực có kính (Trái)", "type": "text" },
                { "name": "visionWithGlassesRight", "label": "Thị lực có kính (Phải)", "type": "text" },
                { "name": "colorBlindness", "label": "Sắc giác (Mù màu)", "type": "select", "options": ["Bình thường", "Mù màu đỏ", "Mù màu xanh", "Mù màu toàn bộ"], "default": "Bình thường" },
                { "name": "eyePathology", "label": "Bệnh lý mắt", "type": "text", "default": "Bình thường" }
              ]
            }
            """
        };

        var stationEnt = new ExamStation
        {
            Code = "ST_ENT",
            Name = "Trạm Tai Mũi Họng",
            SpecialtyCode = "TMH",
            RoomNumber = "P.104",
            DefaultDoctorName = "BS. Vũ Quang Huy",
            OrderIndex = 4,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "whisperHearingLeft", "label": "Thính lực nói thầm (Trái)", "type": "text", "default": "5m" },
                { "name": "whisperHearingRight", "label": "Thính lực nói thầm (Phải)", "type": "text", "default": "5m" },
                { "name": "normalHearingLeft", "label": "Thính lực nói thường (Trái)", "type": "text", "default": "5m" },
                { "name": "normalHearingRight", "label": "Thính lực nói thường (Phải)", "type": "text", "default": "5m" },
                { "name": "ear", "label": "Tai", "type": "text", "default": "Màng nhĩ 2 bên sáng, nón sáng rõ" },
                { "name": "nose", "label": "Mũi", "type": "text", "default": "Niêm mạc hồng, khe thông thoáng, không cuốn quá phát" },
                { "name": "throat", "label": "Họng - Thanh quản", "type": "text", "default": "Niêm mạc họng hồng, amidan không sưng viêm" }
              ]
            }
            """
        };

        var stationDental = new ExamStation
        {
            Code = "ST_DENTAL",
            Name = "Trạm Răng Hàm Mặt",
            SpecialtyCode = "RHM",
            RoomNumber = "P.105",
            DefaultDoctorName = "BS. Đỗ Thị Thu Hà",
            OrderIndex = 5,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "upperJaw", "label": "Hàm trên", "type": "text", "default": "Đủ răng, không sâu" },
                { "name": "lowerJaw", "label": "Hàm dưới", "type": "text", "default": "Đủ răng, không sâu" },
                { "name": "dentalPathology", "label": "Bệnh lý răng miệng", "type": "text", "default": "Không có" }
              ]
            }
            """
        };

        var stationSurgery = new ExamStation
        {
            Code = "ST_SURGERY",
            Name = "Trạm Khám Ngoại khoa & Da liễu",
            SpecialtyCode = "NGOAI",
            RoomNumber = "P.106",
            DefaultDoctorName = "BS. Nguyễn Trọng Hải",
            OrderIndex = 6,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "musculoskeletal", "label": "Cơ xương khớp / Vận động", "type": "text", "default": "Vận động các khớp bình thường, không biến dạng" },
                { "name": "skin", "label": "Da liễu", "type": "text", "default": "Da niêm mạc bình thường, không tổn thương" }
              ]
            }
            """
        };

        var stationGynecol = new ExamStation
        {
            Code = "ST_GYNECOL",
            Name = "Trạm Sản Phụ Khoa",
            SpecialtyCode = "SAN_PHU",
            RoomNumber = "P.107",
            DefaultDoctorName = "BS.CKII. Lê Thị Phương",
            OrderIndex = 7,
            FormSchemaJson = """
            {
              "fields": [
                { "name": "vulva", "label": "Âm hộ - Âm đạo", "type": "text", "default": "Bình thường" },
                { "name": "cervix", "label": "Cổ tử cung", "type": "text", "default": "Nhẵn, không loét" },
                { "name": "uterus", "label": "Tử cung & Phần phụ", "type": "text", "default": "Kích thước bình thường, không u cục" }
              ]
            }
            """
        };

        context.ExamStations.AddRange(
            stationPhysical, stationInternal, stationEye, stationEnt,
            stationDental, stationSurgery, stationGynecol
        );
        await context.SaveChangesAsync();

        // 2. ReferenceRanges (Khoảng tham chiếu chuẩn lâm sàng)
        context.ReferenceRanges.AddRange(
            new ReferenceRange { TestCode = "GLU", TestName = "Định lượng Glucose máu", Gender = TargetGender.All, LowerLimit = 3.9m, UpperLimit = 6.4m, Unit = "mmol/L", Description = "Đường huyết lúc đói" },
            new ReferenceRange { TestCode = "URE", TestName = "Định lượng Ure máu", Gender = TargetGender.All, LowerLimit = 2.5m, UpperLimit = 7.5m, Unit = "mmol/L", Description = "Chức năng thận" },
            new ReferenceRange { TestCode = "CREAT_M", TestName = "Định lượng Creatinin máu (Nam)", Gender = TargetGender.Male, LowerLimit = 62m, UpperLimit = 115m, Unit = "µmol/L", Description = "Chức năng lọc thận nam" },
            new ReferenceRange { TestCode = "CREAT_F", TestName = "Định lượng Creatinin máu (Nữ)", Gender = TargetGender.Female, LowerLimit = 53m, UpperLimit = 97m, Unit = "µmol/L", Description = "Chức năng lọc thận nữ" },
            new ReferenceRange { TestCode = "AST", TestName = "Đo hoạt độ AST (GOT)", Gender = TargetGender.All, LowerLimit = 0m, UpperLimit = 40m, Unit = "U/L", Description = "Men gan AST" },
            new ReferenceRange { TestCode = "ALT", TestName = "Đo hoạt độ ALT (GPT)", Gender = TargetGender.All, LowerLimit = 0m, UpperLimit = 40m, Unit = "U/L", Description = "Men gan ALT" },
            new ReferenceRange { TestCode = "CHOL", TestName = "Định lượng Cholesterol toàn phần", Gender = TargetGender.All, LowerLimit = 3.9m, UpperLimit = 5.2m, Unit = "mmol/L", Description = "Mỡ máu" },
            new ReferenceRange { TestCode = "TRIG", TestName = "Định lượng Triglycerid", Gender = TargetGender.All, LowerLimit = 0.4m, UpperLimit = 1.7m, Unit = "mmol/L", Description = "Mỡ máu triglycerid" },
            new ReferenceRange { TestCode = "HBSAG", TestName = "HBsAg miễn dịch định tính (Viêm gan B)", Gender = TargetGender.All, NormalText = "Âm tính", Description = "Kháng nguyên viêm gan B" }
        );
        await context.SaveChangesAsync();

        // 3. Doanh nghiệp (Companies)
        var compFpt = new Company
        {
            Code = "FPT-CORP",
            Name = "Tập đoàn Công nghệ FPT",
            TaxCode = "0101248141",
            RepresentativeName = "Nguyễn Văn Khoa",
            PhoneNumber = "02473007300",
            Email = "contact@fpt.com.vn",
            Address = "Tòa nhà FPT, Phố Duy Tân, Cầu Giấy, Hà Nội",
            Industry = "Công nghệ thông tin & Viễn thông"
        };

        var compVinamilk = new Company
        {
            Code = "VNM-CORP",
            Name = "Công ty Cổ phần Sữa Việt Nam (Vinamilk)",
            TaxCode = "0300588569",
            RepresentativeName = "Mai Kiều Liên",
            PhoneNumber = "02854155555",
            Email = "vinamilk@vinamilk.com.vn",
            Address = "Số 10 Tân Trào, Phường Tân Phú, Quận 7, TP. Hồ Chí Minh",
            Industry = "Chế biến thực phẩm & đồ uống"
        };

        context.Companies.AddRange(compFpt, compVinamilk);
        await context.SaveChangesAsync();

        // 4. Hợp đồng khám (ExamContracts)
        var contractFpt2026 = new ExamContract
        {
            ContractNumber = "HĐ-2026/01-FPT",
            CompanyId = compFpt.Id,
            Title = "Khám sức khỏe định kỳ năm 2026 cho Cán bộ nhân viên FPT",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(15),
            ExamLocation = ExamLocationType.AtHospital,
            EstimatedPatientCount = 350,
            Status = ContractStatus.InProgress,
            TotalContractAmount = 450_000_000m,
            DiscountAmount = 30_000_000m,
            FinalAmount = 420_000_000m,
            TotalPaidAmount = 210_000_000m,
            Note = "Khám tại cơ sở 1 từ 7h30 sáng"
        };

        var contractVnm2026 = new ExamContract
        {
            ContractNumber = "HĐ-2026/02-VNM",
            CompanyId = compVinamilk.Id,
            Title = "Khám sức khỏe thẻ xanh và định kỳ công nhân nhà máy Vinamilk",
            StartDate = DateTime.UtcNow.AddDays(20),
            EndDate = DateTime.UtcNow.AddDays(25),
            ExamLocation = ExamLocationType.OnSite,
            LocationAddress = "Nhà máy sữa Vinamilk Thống Nhất, KCN Sóng Thần, Bình Dương",
            EstimatedPatientCount = 200,
            Status = ContractStatus.Active,
            TotalContractAmount = 280_000_000m,
            DiscountAmount = 15_000_000m,
            FinalAmount = 265_000_000m,
            TotalPaidAmount = 100_000_000m,
            Note = "Đoàn khám lưu động 15 bác sĩ & điều dưỡng"
        };

        context.ExamContracts.AddRange(contractFpt2026, contractVnm2026);
        await context.SaveChangesAsync();

        // 5. Thanh toán hợp đồng (ContractPayments)
        context.ContractPayments.AddRange(
            new ContractPayment
            {
                ContractId = contractFpt2026.Id,
                PaymentCode = "TT-FPT-01",
                Amount = 210_000_000m,
                PaymentDate = DateTime.UtcNow.AddDays(-2),
                PaymentMethod = PaymentMethod.BankTransfer,
                TransactionReference = "VCB.20260312.98124",
                Note = "Tạm ứng đợt 1 (50% giá trị hợp đồng)"
            },
            new ContractPayment
            {
                ContractId = contractVnm2026.Id,
                PaymentCode = "TT-VNM-01",
                Amount = 100_000_000m,
                PaymentDate = DateTime.UtcNow.AddDays(-1),
                PaymentMethod = PaymentMethod.BankTransfer,
                TransactionReference = "BIDV.20260315.45129",
                Note = "Tạm ứng đợt 1"
            }
        );
        await context.SaveChangesAsync();

        // 6. Gói khám (ExamPackages)
        var pkgMaleStandard = new ExamPackage
        {
            ContractId = contractFpt2026.Id,
            PackageCode = "GOI-FPT-NAM",
            PackageName = "Gói khám KSK định kỳ Nam (Tiêu chuẩn FPT)",
            TargetGender = TargetGender.Male,
            BasePrice = 1_450_000m,
            PackagePrice = 1_200_000m,
            Description = "Bao gồm khám thể lực, 5 chuyên khoa, X-quang phổi, Siêu âm bụng, Xét nghiệm máu & nước tiểu"
        };

        var pkgFemaleStandard = new ExamPackage
        {
            ContractId = contractFpt2026.Id,
            PackageCode = "GOI-FPT-NU",
            PackageName = "Gói khám KSK định kỳ Nữ (Tiêu chuẩn FPT)",
            TargetGender = TargetGender.Female,
            BasePrice = 1_850_000m,
            PackagePrice = 1_500_000m,
            Description = "Bao gồm khám thể lực, 6 chuyên khoa (kèm sản phụ khoa), X-quang phổi, Siêu âm bụng, Xét nghiệm máu & nước tiểu"
        };

        var pkgDriverStandard = new ExamPackage
        {
            ContractId = null, // Gói mẫu mở rộng dùng cho đăng ký lẻ
            PackageCode = "GOI-LAIXE-B2",
            PackageName = "Gói khám sức khỏe lái xe ô tô hạng B2/C (TT 24/2015)",
            TargetGender = TargetGender.All,
            BasePrice = 650_000m,
            PackagePrice = 550_000m,
            Description = "Theo quy định Thông tư liên tịch 24/2015: Đo thị lực, test ma túy 4 chân, nồng độ cồn, điện tim"
        };

        context.ExamPackages.AddRange(pkgMaleStandard, pkgFemaleStandard, pkgDriverStandard);
        await context.SaveChangesAsync();

        // 7. Gán dịch vụ vào gói khám (PackageServices)
        context.PackageServices.AddRange(
            // Dịch vụ gói Nam
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_PHYSICAL", ServiceName = "Khám thể lực & dấu hiệu sinh tồn", ExamStationId = stationPhysical.Id, Price = 50_000m, OrderIndex = 1 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_INTERNAL", ServiceName = "Khám Nội tổng quát", ExamStationId = stationInternal.Id, Price = 100_000m, OrderIndex = 2 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_EYE", ServiceName = "Khám Mắt", ExamStationId = stationEye.Id, Price = 80_000m, OrderIndex = 3 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_ENT", ServiceName = "Khám Tai Mũi Họng", ExamStationId = stationEnt.Id, Price = 80_000m, OrderIndex = 4 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_DENTAL", ServiceName = "Khám Răng Hàm Mặt", ExamStationId = stationDental.Id, Price = 80_000m, OrderIndex = 5 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_SURGERY", ServiceName = "Khám Ngoại khoa & Da liễu", ExamStationId = stationSurgery.Id, Price = 80_000m, OrderIndex = 6 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "XQ_CHEST", ServiceName = "Chụp X-quang tim phổi thẳng", ServiceType = LabOrderType.Imaging, Price = 150_000m, OrderIndex = 7 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "US_ABDOMEN", ServiceName = "Siêu âm ổ bụng tổng quát", ServiceType = LabOrderType.Imaging, Price = 180_000m, OrderIndex = 8 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "LAB_CBC", ServiceName = "Tổng phân tích tế bào máu", ServiceType = LabOrderType.Laboratory, Price = 90_000m, OrderIndex = 9 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "LAB_GLU", ServiceName = "Định lượng Glucose máu", ServiceType = LabOrderType.Laboratory, Price = 50_000m, OrderIndex = 10 },
            new PackageService { PackageId = pkgMaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "LAB_LIVER", ServiceName = "Đo hoạt độ men gan (AST, ALT)", ServiceType = LabOrderType.Laboratory, Price = 100_000m, OrderIndex = 11 },

            // Dịch vụ gói Nữ (bổ sung trạm Sản Phụ Khoa)
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_PHYSICAL", ServiceName = "Khám thể lực & dấu hiệu sinh tồn", ExamStationId = stationPhysical.Id, Price = 50_000m, OrderIndex = 1 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_INTERNAL", ServiceName = "Khám Nội tổng quát", ExamStationId = stationInternal.Id, Price = 100_000m, OrderIndex = 2 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_EYE", ServiceName = "Khám Mắt", ExamStationId = stationEye.Id, Price = 80_000m, OrderIndex = 3 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_ENT", ServiceName = "Khám Tai Mũi Họng", ExamStationId = stationEnt.Id, Price = 80_000m, OrderIndex = 4 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_DENTAL", ServiceName = "Khám Răng Hàm Mặt", ExamStationId = stationDental.Id, Price = 80_000m, OrderIndex = 5 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "ST_GYNECOL", ServiceName = "Khám Sản phụ khoa", ExamStationId = stationGynecol.Id, Price = 120_000m, OrderIndex = 6 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "XQ_CHEST", ServiceName = "Chụp X-quang tim phổi thẳng", ServiceType = LabOrderType.Imaging, Price = 150_000m, OrderIndex = 7 },
            new PackageService { PackageId = pkgFemaleStandard.Id, MedicalServiceId = Guid.NewGuid(), ServiceCode = "US_ABDOMEN", ServiceName = "Siêu âm ổ bụng tổng quát", ServiceType = LabOrderType.Imaging, Price = 180_000m, OrderIndex = 8 }
        );
        await context.SaveChangesAsync();

        // 8. Hồ sơ người khám mẫu (ExamRegistrations & Visits)
        var reg1 = new ExamRegistration
        {
            RegistrationCode = "DK2026-00001",
            ContractId = contractFpt2026.Id,
            PackageId = pkgMaleStandard.Id,
            FullName = "NGUYỄN VĂN AN",
            DateOfBirth = new DateTime(1988, 5, 12, 0, 0, 0, DateTimeKind.Utc),
            Gender = TargetGender.Male,
            IdentityCardNumber = "001088001234",
            PhoneNumber = "0988112233",
            EmployeeCode = "FPT-1042",
            DepartmentName = "Khối Phần mềm FSOFT",
            Position = "Kỹ sư Cầu nối (BrSE)",
            ExamType = ExamType.Corporate,
            Status = RegistrationStatus.WaitingForConclusion,
            TotalAmount = 1_200_000m
        };

        var reg2 = new ExamRegistration
        {
            RegistrationCode = "DK2026-00002",
            ContractId = contractFpt2026.Id,
            PackageId = pkgFemaleStandard.Id,
            FullName = "TRẦN THỊ MAI",
            DateOfBirth = new DateTime(1992, 9, 25, 0, 0, 0, DateTimeKind.Utc),
            Gender = TargetGender.Female,
            IdentityCardNumber = "001192005678",
            PhoneNumber = "0977223344",
            EmployeeCode = "FPT-2015",
            DepartmentName = "Ban Nhân sự & Tuyển dụng",
            Position = "Chuyên viên Tuyển dụng",
            ExamType = ExamType.Corporate,
            Status = RegistrationStatus.InProgress,
            TotalAmount = 1_500_000m
        };

        var regDriver = new ExamRegistration
        {
            RegistrationCode = "DK2026-00003",
            ContractId = null,
            PackageId = pkgDriverStandard.Id,
            FullName = "LÊ HOÀNG NAM",
            DateOfBirth = new DateTime(1995, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender = TargetGender.Male,
            IdentityCardNumber = "079095009988",
            PhoneNumber = "0909123456",
            ExamType = ExamType.Driver,
            DriverLicenseClass = "B2",
            Status = RegistrationStatus.CheckedIn,
            TotalAmount = 550_000m,
            PaidAmount = 550_000m
        };

        context.ExamRegistrations.AddRange(reg1, reg2, regDriver);
        await context.SaveChangesAsync();

        // 9. Lượt khám (ExamVisit) cho reg1 (Đang chờ kết luận)
        var visit1 = new ExamVisit
        {
            RegistrationId = reg1.Id,
            VisitCode = "LK2026-00001",
            CheckInTime = DateTime.UtcNow.AddHours(-3),
            QueueNumber = 1,
            Status = VisitStatus.WaitingForConclusion,
            HeightCm = 172.5m,
            WeightKg = 68.0m,
            Bmi = 22.8m,
            PulseRate = 76,
            BloodPressureSystolic = 118,
            BloodPressureDiastolic = 78
        };

        context.ExamVisits.Add(visit1);
        await context.SaveChangesAsync();

        // Kết quả các trạm của visit1
        context.StationResults.AddRange(
            new StationResult
            {
                VisitId = visit1.Id,
                StationId = stationPhysical.Id,
                DoctorName = "ĐD. Nguyễn Thu Trang",
                ExaminedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(-40),
                Status = StationStatus.Completed,
                ResultDataJson = """{"heightCm": 172.5, "weightKg": 68.0, "bmi": 22.8, "pulse": 76, "bpSystolic": 118, "bpDiastolic": 78}""",
                ClinicalConclusion = "Thể lực tốt, chỉ số BMI và huyết áp bình thường",
                SpecialtyClassification = HealthClassification.Type1
            },
            new StationResult
            {
                VisitId = visit1.Id,
                StationId = stationInternal.Id,
                DoctorName = "BS.CKI. Trần Minh Tuấn",
                ExaminedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(-10),
                Status = StationStatus.Completed,
                ResultDataJson = """{"cardiovascular": "Bình thường", "respiratory": "Bình thường", "digestive": "Bình thường"}""",
                ClinicalConclusion = "Hệ tuần hoàn, hô hấp, tiêu hóa bình thường",
                SpecialtyClassification = HealthClassification.Type1
            },
            new StationResult
            {
                VisitId = visit1.Id,
                StationId = stationEye.Id,
                DoctorName = "BS.CKI. Hoàng Mai Lan",
                ExaminedAt = DateTime.UtcNow.AddHours(-1).AddMinutes(-30),
                Status = StationStatus.Completed,
                ResultDataJson = """{"visionWithoutGlassesLeft": "10/10", "visionWithoutGlassesRight": "10/10", "colorBlindness": "Bình thường"}""",
                ClinicalConclusion = "Thị lực 2 mắt 10/10, sắc giác tốt",
                SpecialtyClassification = HealthClassification.Type1
            }
        );

        // LabOrder của visit1
        var labOrder1 = new LabOrder
        {
            VisitId = visit1.Id,
            OrderCode = "CLS2026-00001",
            OrderType = LabOrderType.Laboratory,
            OrderedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(-45),
            OrderedByDoctorName = "BS.CKI. Trần Minh Tuấn",
            Status = LabOrderStatus.Completed,
            SampleCode = "BC-88991"
        };
        context.LabOrders.Add(labOrder1);
        await context.SaveChangesAsync();

        context.LabResults.AddRange(
            new LabResult
            {
                LabOrderId = labOrder1.Id,
                MedicalServiceId = Guid.NewGuid(),
                TestCode = "GLU",
                TestName = "Định lượng Glucose máu",
                Value = "5.2",
                NumericValue = 5.2m,
                Unit = "mmol/L",
                NormalRangeText = "3.9 - 6.4",
                IsAbnormal = false,
                AbnormalFlag = AbnormalFlag.Normal,
                PerformedAt = DateTime.UtcNow.AddHours(-1),
                PerformedBy = "KTV. Lê Thị Hà"
            },
            new LabResult
            {
                LabOrderId = labOrder1.Id,
                MedicalServiceId = Guid.NewGuid(),
                TestCode = "AST",
                TestName = "Đo hoạt độ AST (GOT)",
                Value = "28",
                NumericValue = 28m,
                Unit = "U/L",
                NormalRangeText = "0 - 40",
                IsAbnormal = false,
                AbnormalFlag = AbnormalFlag.Normal,
                PerformedAt = DateTime.UtcNow.AddHours(-1),
                PerformedBy = "KTV. Lê Thị Hà"
            }
        );

        await context.SaveChangesAsync();
    }
}

