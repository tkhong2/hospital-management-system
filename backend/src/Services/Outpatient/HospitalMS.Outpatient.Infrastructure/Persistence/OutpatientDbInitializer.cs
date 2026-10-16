using HospitalMS.Outpatient.Domain.Entities;
using HospitalMS.Outpatient.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalMS.Outpatient.Infrastructure.Persistence;

public static class OutpatientDbInitializer
{
    public static async Task SeedAsync(OutpatientDbContext db, ILogger logger)
    {
        try
        {
            if (await db.Receptions.AnyAsync()) return;

            var patient1Id = Guid.NewGuid();
            var patient2Id = Guid.NewGuid();
            var clinicRoomId = Guid.NewGuid();

            var r1 = new OutpatientReception
            {
                QueueNumber = 1,
                PatientId = patient1Id,
                PatientCode = "BN26000001",
                PatientName = "Nguyễn Văn An",
                DateOfBirth = new DateTime(1985, 4, 15),
                Gender = "Male",
                PhoneNumber = "0912345678",
                Address = "Ba Đình, Hà Nội",
                HealthInsuranceNumber = "DN4010123456789",
                ClinicRoomId = clinicRoomId,
                ClinicRoomName = "Phòng khám Nội 01",
                PatientType = PatientType.OutpatientInsurance,
                Status = ReceptionStatus.InConsultation,
                ReceptionTime = DateTime.UtcNow.AddMinutes(-30),
                Reason = "Đau đầu, chóng mặt kéo dài",
                Priority = 0
            };

            var r2 = new OutpatientReception
            {
                QueueNumber = 2,
                PatientId = patient2Id,
                PatientCode = "BN26000002",
                PatientName = "Trần Thị Bích",
                DateOfBirth = new DateTime(1992, 8, 20),
                Gender = "Female",
                PhoneNumber = "0987654321",
                Address = "Cầu Giấy, Hà Nội",
                HealthInsuranceNumber = "GD4010198765432",
                ClinicRoomId = clinicRoomId,
                ClinicRoomName = "Phòng khám Nội 01",
                PatientType = PatientType.OutpatientInsurance,
                Status = ReceptionStatus.Waiting,
                ReceptionTime = DateTime.UtcNow.AddMinutes(-15),
                Reason = "Sốt cao, đau họng, mệt mỏi",
                Priority = 1
            };

            db.Receptions.AddRange(r1, r2);

            var encounter1 = new OutpatientEncounter
            {
                ReceptionId = r1.Id,
                PatientId = patient1Id,
                DoctorId = "dr_tung",
                DoctorName = "BS.CKII Nguyễn Thanh Tùng",
                ClinicRoomName = "Phòng khám Nội 01",
                StartTime = DateTime.UtcNow.AddMinutes(-20),
                ChiefComplaint = "Đau đầu, hoa mắt chóng mặt khi thay đổi tư thế",
                HistoryOfPresentIllness = "Bệnh nhân bị đau đầu vùng chẩm 3 ngày nay, kèm theo cảm giác bồng bềnh hoa mắt",
                MedicalHistory = "Tăng huyết áp 2 năm điều trị không liên tục, không dị ứng thuốc",
                PhysicalExam = "Tim đều, phổi trong, thần kinh tỉnh táo, không liệt khu trú",
                Pulse = 78,
                BloodPressureSystolic = 145,
                BloodPressureDiastolic = 90,
                Temperature = 36.8m,
                SpO2 = 98,
                Weight = 68,
                Height = 168,
                PrimaryIcd10Code = "I10",
                PrimaryIcd10Name = "Bệnh tăng huyết áp vô căn (nguyên phát)",
                SecondaryIcd10Code = "R42",
                SecondaryIcd10Name = "Chóng mặt và choáng váng",
                ClinicalDiagnosis = "Tăng huyết áp độ 1 / Cơn thiếu máu não thoáng qua",
                Disposition = TreatmentDisposition.DischargedWithPrescription,
                DoctorNotes = "Nghỉ ngơi, giảm ăn mặn, tái khám sau 7 ngày hoặc khi huyết áp >= 160/100",
                FollowUpDate = DateTime.UtcNow.AddDays(7)
            };

            // Thêm chỉ định CLS
            var order1 = new OutpatientServiceOrder
            {
                EncounterId = encounter1.Id,
                PatientId = patient1Id,
                OrderCode = "CLS-261017-1001",
                OrderDate = DateTime.UtcNow.AddMinutes(-18),
                DoctorName = "BS.CKII Nguyễn Thanh Tùng",
                Status = ServiceOrderStatus.Completed,
                TotalAmount = 250000m,
                TotalInsuranceAmount = 200000m,
                TotalPatientAmount = 50000m
            };

            order1.Details.Add(new OutpatientServiceOrderDetail
            {
                ServiceId = Guid.NewGuid(),
                ServiceCode = "XN-CTM",
                ServiceName = "Tổng phân tích tế bào máu ngoại vi bằng máy đếm laser",
                ServiceGroup = "XET_NGHIEM",
                Quantity = 1,
                UnitPrice = 100000m,
                InsuranceCoverageRate = 0.8m,
                InsuranceCoveredAmount = 80000m,
                PatientPayAmount = 20000m,
                RoomToExecute = "Phòng Xét Nghiệm Trung Tâm",
                ResultSummary = "RBC: 4.8 T/L, WBC: 7.2 G/L, PLT: 250 G/L (Trong giới hạn bình thường)",
                ResultTime = DateTime.UtcNow.AddMinutes(-5),
                IsAbnormal = false
            });

            order1.Details.Add(new OutpatientServiceOrderDetail
            {
                ServiceId = Guid.NewGuid(),
                ServiceCode = "TDCN-ECG",
                ServiceName = "Ghi điện tim cấp cứu tại giường",
                ServiceGroup = "THAM_DO_CN",
                Quantity = 1,
                UnitPrice = 150000m,
                InsuranceCoverageRate = 0.8m,
                InsuranceCoveredAmount = 120000m,
                PatientPayAmount = 30000m,
                RoomToExecute = "Phòng Thăm Dò Chức Năng 02",
                ResultSummary = "Nhịp xoang đều 78ck/p, trục trung gian, không thiếu máu cơ tim",
                ResultTime = DateTime.UtcNow.AddMinutes(-5),
                IsAbnormal = false
            });

            // Thêm đơn thuốc
            var pres1 = new OutpatientPrescription
            {
                EncounterId = encounter1.Id,
                PatientId = patient1Id,
                PrescriptionCode = "TOA-261017-2001",
                PrescriptionDate = DateTime.UtcNow.AddMinutes(-2),
                DoctorName = "BS.CKII Nguyễn Thanh Tùng",
                Diagnosis = "Tăng huyết áp vô căn (nguyên phát)",
                DaysCount = 14,
                Advice = "Uống thuốc đều đặn vào mỗi buổi sáng sau khi ăn",
                Status = PrescriptionStatus.Prescribed,
                TotalAmount = 280000m,
                TotalInsuranceAmount = 224000m,
                TotalPatientAmount = 56000m
            };

            pres1.Items.Add(new OutpatientPrescriptionItem
            {
                MedicineId = Guid.NewGuid(),
                MedicineCode = "MED-AMLODIPINE",
                MedicineName = "Amlodipin 5mg",
                ActiveIngredient = "Amlodipine besilate",
                DosageForm = "Viên nén",
                Quantity = 14,
                MorningDose = "1 viên",
                NoonDose = "",
                AfternoonDose = "",
                NightDose = "",
                UsageInstructions = "Uống 1 viên vào 8h sáng sau ăn",
                UnitPrice = 10000m,
                IsInsuranceCovered = true,
                InsuranceCoveredAmount = 112000m,
                PatientPayAmount = 28000m
            });

            pres1.Items.Add(new OutpatientPrescriptionItem
            {
                MedicineId = Guid.NewGuid(),
                MedicineCode = "MED-TANAKAN",
                MedicineName = "Tanakan 40mg",
                ActiveIngredient = "Ginkgo biloba extract",
                DosageForm = "Viên bao phim",
                Quantity = 28,
                MorningDose = "1 viên",
                NoonDose = "",
                AfternoonDose = "1 viên",
                NightDose = "",
                UsageInstructions = "Uống sáng 1 viên, chiều 1 viên",
                UnitPrice = 5000m,
                IsInsuranceCovered = true,
                InsuranceCoveredAmount = 112000m,
                PatientPayAmount = 28000m
            });

            encounter1.ServiceOrders.Add(order1);
            encounter1.Prescriptions.Add(pres1);
            db.Encounters.Add(encounter1);

            await db.SaveChangesAsync();
            logger.LogInformation("[OutpatientDbInitializer] Seeded sample outpatient reception, encounter, orders and prescription successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[OutpatientDbInitializer] Error while seeding outpatient data.");
        }
    }
}

