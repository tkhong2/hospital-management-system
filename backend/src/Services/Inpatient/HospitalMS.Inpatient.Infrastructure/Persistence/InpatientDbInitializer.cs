using HospitalMS.Inpatient.Domain.Entities;
using HospitalMS.Inpatient.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Inpatient.Infrastructure.Persistence;

public static class InpatientDbInitializer
{
    public static async Task SeedAsync(InpatientDbContext db, ILogger logger)
    {
        try
        {
            if (await db.Rooms.AnyAsync()) return;

            // 1. Khoa Nội - Phòng P201
            var roomP201 = new DepartmentRoom
            {
                DepartmentCode = "KHOA_NOI",
                DepartmentName = "Khoa Nội Tổng Hợp",
                RoomNumber = "P201",
                RoomName = "Phòng Hồi Sức Tích Cực Nội",
                Floor = 2,
                Notes = "Phòng trang bị hệ thống oxy trung tâm và monitor theo dõi"
            };

            var bed1 = new HospitalBed
            {
                Room = roomP201,
                BedCode = "G-201-01",
                BedName = "Giường 01 (ICU)",
                BedType = BedType.Emergency,
                Status = BedStatus.Occupied,
                DailyPrice = 450000m,
                InsuranceCoveredPrice = 350000m
            };

            var bed2 = new HospitalBed
            {
                Room = roomP201,
                BedCode = "G-201-02",
                BedName = "Giường 02 (ICU)",
                BedType = BedType.Emergency,
                Status = BedStatus.Empty,
                DailyPrice = 450000m,
                InsuranceCoveredPrice = 350000m
            };

            var bed3 = new HospitalBed
            {
                Room = roomP201,
                BedCode = "G-201-03",
                BedName = "Giường 03 (ICU)",
                BedType = BedType.Emergency,
                Status = BedStatus.Cleaning,
                DailyPrice = 450000m,
                InsuranceCoveredPrice = 350000m,
                Notes = "Vừa xuất viện lúc 09h00, đang khử khuẩn"
            };

            roomP201.Beds.Add(bed1);
            roomP201.Beds.Add(bed2);
            roomP201.Beds.Add(bed3);

            // 2. Khoa Nội - Phòng P202
            var roomP202 = new DepartmentRoom
            {
                DepartmentCode = "KHOA_NOI",
                DepartmentName = "Khoa Nội Tổng Hợp",
                RoomNumber = "P202",
                RoomName = "Phòng Điều Trị Nội Khoa",
                Floor = 2
            };

            var bed4 = new HospitalBed
            {
                Room = roomP202,
                BedCode = "G-202-01",
                BedName = "Giường 01",
                BedType = BedType.Standard,
                Status = BedStatus.Occupied,
                DailyPrice = 250000m,
                InsuranceCoveredPrice = 200000m
            };

            var bed5 = new HospitalBed
            {
                Room = roomP202,
                BedCode = "G-202-02",
                BedName = "Giường 02",
                BedType = BedType.Standard,
                Status = BedStatus.Empty,
                DailyPrice = 250000m,
                InsuranceCoveredPrice = 200000m
            };

            var bed6 = new HospitalBed
            {
                Room = roomP202,
                BedCode = "G-202-03",
                BedName = "Giường 03",
                BedType = BedType.Standard,
                Status = BedStatus.Maintenance,
                DailyPrice = 250000m,
                InsuranceCoveredPrice = 200000m,
                Notes = "Hỏng thanh nâng hạ đầu giường, phòng kỹ thuật đang xử lý"
            };

            roomP202.Beds.Add(bed4);
            roomP202.Beds.Add(bed5);
            roomP202.Beds.Add(bed6);

            // 3. Khoa Ngoại - Phòng P301
            var roomP301 = new DepartmentRoom
            {
                DepartmentCode = "KHOA_NGOAI",
                DepartmentName = "Khoa Ngoại Tổng Hợp",
                RoomNumber = "P301",
                RoomName = "Phòng Hậu Phẫu Ngoại",
                Floor = 3
            };

            var bed7 = new HospitalBed
            {
                Room = roomP301,
                BedCode = "G-301-01",
                BedName = "Giường 01",
                BedType = BedType.Standard,
                Status = BedStatus.Occupied,
                DailyPrice = 300000m,
                InsuranceCoveredPrice = 240000m
            };

            var bed8 = new HospitalBed
            {
                Room = roomP301,
                BedCode = "G-301-02",
                BedName = "Giường 02",
                BedType = BedType.Standard,
                Status = BedStatus.Empty,
                DailyPrice = 300000m,
                InsuranceCoveredPrice = 240000m
            };

            roomP301.Beds.Add(bed7);
            roomP301.Beds.Add(bed8);

            db.Rooms.AddRange(roomP201, roomP202, roomP301);

            // Seed Bệnh nhân 1 (nằm giường G-201-01)
            var patient1Id = Guid.NewGuid();
            var adm1 = new InpatientAdmission
            {
                AdmissionCode = "BA-261017-001",
                PatientId = patient1Id,
                PatientCode = "BN26000010",
                PatientName = "Lê Hoàng Long",
                DateOfBirth = new DateTime(1968, 5, 12),
                Gender = "Male",
                HealthInsuranceNumber = "HT2010199887766",
                RelativeContact = "Lê Thị Lan (Con gái) - 0918889999",
                AdmissionTime = DateTime.UtcNow.AddDays(-2),
                DepartmentCode = "KHOA_NOI",
                DepartmentName = "Khoa Nội Tổng Hợp",
                CurrentBedId = bed1.Id,
                CurrentBedCode = bed1.BedCode,
                AdmittingDoctorName = "BS.CKII Trần Đức Cường",
                InitialDiagnosis = "Viêm phổi thùy cấp / Đái tháo đường type 2",
                PrimaryIcd10Code = "J18.9",
                PrimaryIcd10Name = "Viêm phổi không đặc hiệu",
                Status = AdmissionStatus.Admitted,
                TotalAdvancePayment = 5000000m,
                TotalInpatientExpenses = 3200000m
            };

            bed1.CurrentAdmissionId = adm1.Id;

            // Y lệnh điều trị
            adm1.MedicalOrders.Add(new InpatientMedicalOrder
            {
                OrderDate = DateTime.UtcNow.AddHours(-10),
                DoctorName = "BS.CKII Trần Đức Cường",
                Diagnosis = "Viêm phổi thùy cấp, đái tháo đường type 2",
                CareLevel = CareLevel.Level1,
                DietPlan = "Chế độ ăn cơm bệnh lý đái tháo đường, hạn chế tinh bột đường",
                MedicinesContent = "1. Ceftriaxon 1g x 02 lọ (Tiêm TMC lúc 08h - 20h)\n2. Paracetamol 500mg x 02 viên (Uống khi sốt >= 38.5)\n3. Mixtard 30 100IU/ml x 20 đơn vị tiêm dưới da trước ăn sáng",
                ServicesContent = "Khí dung Pulmicort 0.5mg x 2 cữ/ngày",
                SpecialInstructions = "Theo dõi SpO2 liên tục, báo bác sĩ ngay nếu SpO2 < 93%",
                IsExecuted = true,
                ExecutedByNurse = "ĐD. Phạm Thị Mai",
                ExecutedTime = DateTime.UtcNow.AddHours(-9)
            });

            // Theo dõi sinh hiệu
            adm1.VitalSigns.Add(new InpatientVitalSign
            {
                RecordTime = DateTime.UtcNow.AddHours(-8),
                Pulse = 86,
                Temperature = 38.2m,
                BloodPressureSystolic = 135,
                BloodPressureDiastolic = 85,
                BreathingRate = 22,
                SpO2 = 95,
                Weight = 65,
                Notes = "Bệnh nhân còn sốt nhẹ, ho có đờm vàng",
                RecordedByNurse = "ĐD. Phạm Thị Mai"
            });

            adm1.VitalSigns.Add(new InpatientVitalSign
            {
                RecordTime = DateTime.UtcNow.AddHours(-2),
                Pulse = 78,
                Temperature = 37.1m,
                BloodPressureSystolic = 125,
                BloodPressureDiastolic = 80,
                BreathingRate = 19,
                SpO2 = 97,
                Weight = 65,
                Notes = "Đã hạ sốt, hô hấp cải thiện",
                RecordedByNurse = "ĐD. Nguyễn Hoàng Nam"
            });

            // Phiếu chăm sóc
            adm1.CareNotes.Add(new InpatientCareNote
            {
                NoteTime = DateTime.UtcNow.AddHours(-4),
                ClinicalEvolution = "Bệnh nhân tỉnh, tiếp xúc tốt, giảm khó thở, ho giảm đờm",
                NursingActions = "Đã cho bệnh nhân thở oxy gọng kính 2L/phút, thực hiện y lệnh kháng sinh và khí dung đúng giờ, đo sinh hiệu",
                Evaluation = "Bệnh nhân đỡ mệt, các chỉ số sinh hiệu trong tầm kiểm soát",
                NurseName = "ĐD. Phạm Thị Mai"
            });

            // Tạm ứng
            adm1.AdvancePayments.Add(new InpatientAdvancePayment
            {
                ReceiptNumber = "TU-261015-001",
                PaymentDate = DateTime.UtcNow.AddDays(-2),
                Amount = 5000000m,
                PaymentMethod = "TRANSFER",
                CashierName = "Thu ngân Trần Thu Hà",
                Notes = "Tạm ứng đợt 1 lúc nhập viện"
            });

            // Bệnh nhân 2 (nằm giường G-202-01)
            var patient2Id = Guid.NewGuid();
            var adm2 = new InpatientAdmission
            {
                AdmissionCode = "BA-261017-002",
                PatientId = patient2Id,
                PatientCode = "BN26000015",
                PatientName = "Hoàng Kim Ngân",
                DateOfBirth = new DateTime(1995, 11, 3),
                Gender = "Female",
                HealthInsuranceNumber = "DN4010177665544",
                RelativeContact = "Hoàng Văn Tuấn (Bố) - 0903334445",
                AdmissionTime = DateTime.UtcNow.AddDays(-1),
                DepartmentCode = "KHOA_NOI",
                DepartmentName = "Khoa Nội Tổng Hợp",
                CurrentBedId = bed4.Id,
                CurrentBedCode = bed4.BedCode,
                AdmittingDoctorName = "BS.CKI Vũ Đình Huy",
                InitialDiagnosis = "Viêm dạ dày ruột cấp tính có mất nước",
                PrimaryIcd10Code = "K52.9",
                PrimaryIcd10Name = "Viêm dạ dày - ruột và đại tràng không nhiễm trùng",
                Status = AdmissionStatus.Admitted,
                TotalAdvancePayment = 3000000m,
                TotalInpatientExpenses = 1500000m
            };

            bed4.CurrentAdmissionId = adm2.Id;

            // Bệnh nhân 3 (nằm giường G-301-01 Ngoại hậu phẫu)
            var patient3Id = Guid.NewGuid();
            var adm3 = new InpatientAdmission
            {
                AdmissionCode = "BA-261017-003",
                PatientId = patient3Id,
                PatientCode = "BN26000022",
                PatientName = "Đỗ Quốc Bảo",
                DateOfBirth = new DateTime(1982, 3, 27),
                Gender = "Male",
                HealthInsuranceNumber = "CA4010188223344",
                RelativeContact = "Vũ Thu Trang (Vợ) - 0978999888",
                AdmissionTime = DateTime.UtcNow.AddDays(-3),
                DepartmentCode = "KHOA_NGOAI",
                DepartmentName = "Khoa Ngoại Tổng Hợp",
                CurrentBedId = bed7.Id,
                CurrentBedCode = bed7.BedCode,
                AdmittingDoctorName = "BS.CKII Lê Văn Dũng",
                InitialDiagnosis = "Hậu phẫu ngày thứ 2 nội soi cắt ruột thừa viêm",
                PrimaryIcd10Code = "K35.8",
                PrimaryIcd10Name = "Viêm ruột thừa cấp khác và không đặc hiệu",
                Status = AdmissionStatus.Admitted,
                TotalAdvancePayment = 10000000m,
                TotalInpatientExpenses = 7800000m
            };

            bed7.CurrentAdmissionId = adm3.Id;

            db.Admissions.AddRange(adm1, adm2, adm3);
            await db.SaveChangesAsync();

            logger.LogInformation("[InpatientDbInitializer] Seeded sample inpatient department rooms, beds, admissions, medical orders, vital signs, and care notes successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[InpatientDbInitializer] Error while seeding inpatient data.");
        }
    }
}

