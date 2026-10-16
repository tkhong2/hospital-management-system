using FluentAssertions;
using HospitalMS.Outpatient.Application.Features.Encounters;
using HospitalMS.Outpatient.Application.Features.Orders;
using HospitalMS.Outpatient.Application.Features.Receptions;
using HospitalMS.Outpatient.Domain.Enums;
using HospitalMS.Outpatient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HospitalMS.Outpatient.Tests;

public class OutpatientTests
{
    private OutpatientDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<OutpatientDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new OutpatientDbContext(options);
    }

    [Fact]
    public async Task CreateReception_ShouldAssignAutoIncrementingQueueNumber()
    {
        using var db = CreateDbContext(nameof(CreateReception_ShouldAssignAutoIncrementingQueueNumber));
        var handler = new CreateReceptionHandler(db);
        var clinicRoomId = Guid.NewGuid();

        var cmd1 = new CreateReceptionCommand(
            Guid.NewGuid(), "BN001", "Bệnh nhân 1", null, "Male", null, null, null,
            clinicRoomId, "Phòng 101", PatientType.OutpatientInsurance, "Sốt"
        );
        var res1 = await handler.Handle(cmd1, CancellationToken.None);

        var cmd2 = new CreateReceptionCommand(
            Guid.NewGuid(), "BN002", "Bệnh nhân 2", null, "Female", null, null, null,
            clinicRoomId, "Phòng 101", PatientType.ServiceFee, "Đau bụng"
        );
        var res2 = await handler.Handle(cmd2, CancellationToken.None);

        res1.IsSuccess.Should().BeTrue();
        res1.Value!.QueueNumber.Should().Be(1);
        res2.IsSuccess.Should().BeTrue();
        res2.Value!.QueueNumber.Should().Be(2);
    }

    [Fact]
    public async Task StartEncounter_ShouldCreateEncounterAndSetStatusInConsultation()
    {
        using var db = CreateDbContext(nameof(StartEncounter_ShouldCreateEncounterAndSetStatusInConsultation));
        var receptionHandler = new CreateReceptionHandler(db);
        var recRes = await receptionHandler.Handle(new CreateReceptionCommand(
            Guid.NewGuid(), "BN003", "Bệnh nhân 3", null, "Male", null, null, null,
            Guid.NewGuid(), "Phòng 102", PatientType.OutpatientInsurance, "Đau ngực"
        ), CancellationToken.None);

        var encounterHandler = new StartEncounterHandler(db);
        var res = await encounterHandler.Handle(new StartEncounterCommand(
            recRes.Value!.Id, "dr_01", "Bác sĩ A"
        ), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value!.DoctorName.Should().Be("Bác sĩ A");
        res.Value.ChiefComplaint.Should().Be("Đau ngực");

        var rec = await db.Receptions.FindAsync(recRes.Value.Id);
        rec!.Status.Should().Be(ReceptionStatus.InConsultation);
    }

    [Fact]
    public async Task UpdateEncounterDiagnosis_ShouldSaveIcd10AndVitalSigns()
    {
        using var db = CreateDbContext(nameof(UpdateEncounterDiagnosis_ShouldSaveIcd10AndVitalSigns));
        var recHandler = new CreateReceptionHandler(db);
        var rec = (await recHandler.Handle(new CreateReceptionCommand(
            Guid.NewGuid(), "BN004", "Bệnh nhân 4", null, "Male", null, null, null,
            Guid.NewGuid(), "Phòng 101", PatientType.OutpatientInsurance, "Đau đầu"
        ), CancellationToken.None)).Value!;

        var encHandler = new StartEncounterHandler(db);
        var enc = (await encHandler.Handle(new StartEncounterCommand(rec.Id, "dr_01", "Bác sĩ B"), CancellationToken.None)).Value!;

        var updateHandler = new UpdateEncounterDiagnosisHandler(db);
        var res = await updateHandler.Handle(new UpdateEncounterDiagnosisCommand(
            enc.Id, "Đau đầu dữ dội", "Đau 2 ngày", "Tiền sử tăng HA", "Tim đều",
            Pulse: 80, BloodPressureSystolic: 140, BloodPressureDiastolic: 90,
            Temperature: 37.0m, SpO2: 98, Weight: 70, Height: 170,
            PrimaryIcd10Code: "I10", PrimaryIcd10Name: "Tăng huyết áp vô căn",
            SecondaryIcd10Code: null, SecondaryIcd10Name: null,
            ClinicalDiagnosis: "Tăng huyết áp độ 1",
            Disposition: TreatmentDisposition.DischargedWithPrescription,
            DoctorNotes: "Ăn nhạt, nghỉ ngơi",
            FollowUpDate: DateTime.UtcNow.AddDays(7)
        ), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value!.PrimaryIcd10Code.Should().Be("I10");
        res.Value.BloodPressureSystolic.Should().Be(140);
        res.Value.Pulse.Should().Be(80);
    }

    [Fact]
    public async Task CreateServiceOrder_ShouldCalculateCorrectInsuranceAndPatientShares()
    {
        using var db = CreateDbContext(nameof(CreateServiceOrder_ShouldCalculateCorrectInsuranceAndPatientShares));
        var rec = (await new CreateReceptionHandler(db).Handle(new CreateReceptionCommand(
            Guid.NewGuid(), "BN005", "Bệnh nhân 5", null, "Male", null, null, null,
            Guid.NewGuid(), "Phòng 101", PatientType.OutpatientInsurance, "Khám tổng quát"
        ), CancellationToken.None)).Value!;

        var enc = (await new StartEncounterHandler(db).Handle(new StartEncounterCommand(rec.Id, "dr_01", "Bác sĩ C"), CancellationToken.None)).Value!;

        var orderHandler = new CreateServiceOrderHandler(db);
        var res = await orderHandler.Handle(new CreateServiceOrderCommand(
            enc.Id, "Bác sĩ C",
            new List<CreateServiceItemRequest>
            {
                new(Guid.NewGuid(), "XN01", "Công thức máu", "XET_NGHIEM", 1, 100000m, 0.8m, "P.XN"),
                new(Guid.NewGuid(), "SA01", "Siêu âm bụng", "CDHA", 1, 200000m, 0.8m, "P.SA")
            }
        ), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value!.TotalAmount.Should().Be(300000m);
        res.Value.TotalInsuranceAmount.Should().Be(240000m);
        res.Value.TotalPatientAmount.Should().Be(60000m);

        var updatedRec = await db.Receptions.FindAsync(rec.Id);
        updatedRec!.Status.Should().Be(ReceptionStatus.WaitingForLab);
    }

    [Fact]
    public async Task CreatePrescriptionAndBilling_ShouldGenerateValidInvoices()
    {
        using var db = CreateDbContext(nameof(CreatePrescriptionAndBilling_ShouldGenerateValidInvoices));
        var rec = (await new CreateReceptionHandler(db).Handle(new CreateReceptionCommand(
            Guid.NewGuid(), "BN006", "Bệnh nhân 6", null, "Female", null, null, null,
            Guid.NewGuid(), "Phòng 101", PatientType.OutpatientInsurance, "Sốt phát ban"
        ), CancellationToken.None)).Value!;

        var enc = (await new StartEncounterHandler(db).Handle(new StartEncounterCommand(rec.Id, "dr_01", "Bác sĩ D"), CancellationToken.None)).Value!;

        var presHandler = new CreatePrescriptionHandler(db);
        var presRes = await presHandler.Handle(new CreatePrescriptionCommand(
            enc.Id, "Bác sĩ D", "Sốt siêu vi", 5, "Nghỉ ngơi, uống nhiều nước",
            new List<CreatePrescriptionItemRequest>
            {
                new(Guid.NewGuid(), "MED01", "Paracetamol 500mg", "Paracetamol", "Viên", 10, "1 viên", "1 viên", "", "", "Sau ăn", 2000m, true)
            }
        ), CancellationToken.None);

        presRes.IsSuccess.Should().BeTrue();
        presRes.Value!.TotalAmount.Should().Be(20000m);

        var billHandler = new ProcessOutpatientBillingHandler(db);
        var billRes = await billHandler.Handle(new ProcessOutpatientBillingCommand(
            enc.Id, "CASH", "Thu ngân 01"
        ), CancellationToken.None);

        billRes.IsSuccess.Should().BeTrue();
        billRes.Value!.TotalAmount.Should().BeGreaterThan(0);
        billRes.Value.Status.Should().Be(BillingStatus.Paid);
    }
}

