using FluentAssertions;
using HospitalMS.Inpatient.Application.Features.Admissions;
using HospitalMS.Inpatient.Application.Features.Beds;
using HospitalMS.Inpatient.Application.Features.Clinical;
using HospitalMS.Inpatient.Domain.Entities;
using HospitalMS.Inpatient.Domain.Enums;
using HospitalMS.Inpatient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HospitalMS.Inpatient.Tests;

public class InpatientTests
{
    private InpatientDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InpatientDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new InpatientDbContext(options);
    }

    private async Task<(DepartmentRoom room, HospitalBed bed1, HospitalBed bed2)> SeedRoomAndBeds(InpatientDbContext db)
    {
        var room = new DepartmentRoom
        {
            DepartmentCode = "KHOA_NOI",
            DepartmentName = "Khoa Nội",
            RoomNumber = "P101",
            RoomName = "Phòng Hồi Sức",
            Floor = 1
        };

        var bed1 = new HospitalBed
        {
            Room = room,
            BedCode = "G-101-01",
            BedName = "Giường 01",
            Status = BedStatus.Empty,
            DailyPrice = 200000m
        };

        var bed2 = new HospitalBed
        {
            Room = room,
            BedCode = "G-101-02",
            BedName = "Giường 02",
            Status = BedStatus.Empty,
            DailyPrice = 200000m
        };

        room.Beds.Add(bed1);
        room.Beds.Add(bed2);
        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        return (room, bed1, bed2);
    }

    [Fact]
    public async Task CreateAdmission_ShouldAssignBedAndMarkOccupied()
    {
        using var db = CreateDbContext(nameof(CreateAdmission_ShouldAssignBedAndMarkOccupied));
        var (_, bed1, _) = await SeedRoomAndBeds(db);

        var handler = new CreateAdmissionHandler(db);
        var res = await handler.Handle(new CreateAdmissionCommand(
            Guid.NewGuid(), "BN100", "Nguyễn Văn X", null, "Male", null, null,
            "KHOA_NOI", "Khoa Nội", bed1.Id, "BS. Nam", "Viêm phổi", "J18", "Viêm phổi", 2000000m
        ), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value!.CurrentBedCode.Should().Be("G-101-01");
        res.Value.TotalAdvancePayment.Should().Be(2000000m);

        var bedAfter = await db.Beds.FindAsync(bed1.Id);
        bedAfter!.Status.Should().Be(BedStatus.Occupied);
        bedAfter.CurrentAdmissionId.Should().Be(res.Value.Id);
    }

    [Fact]
    public async Task GetBedMap_ShouldCalculateOccupancyRateAndCountsAccurately()
    {
        using var db = CreateDbContext(nameof(GetBedMap_ShouldCalculateOccupancyRateAndCountsAccurately));
        var (_, bed1, _) = await SeedRoomAndBeds(db);

        // Cho 1 giường có người
        var admHandler = new CreateAdmissionHandler(db);
        await admHandler.Handle(new CreateAdmissionCommand(
            Guid.NewGuid(), "BN101", "Trần Y", null, "Female", null, null,
            "KHOA_NOI", "Khoa Nội", bed1.Id, "BS. Nam", "Sốt rét", "B54", "Sốt rét", 0m
        ), CancellationToken.None);

        var bedMapHandler = new GetBedMapHandler(db);
        var mapRes = await bedMapHandler.Handle(new GetBedMapQuery("KHOA_NOI"), CancellationToken.None);

        mapRes.IsSuccess.Should().BeTrue();
        mapRes.Value!.TotalBeds.Should().Be(2);
        mapRes.Value.OccupiedBeds.Should().Be(1);
        mapRes.Value.EmptyBeds.Should().Be(1);
        mapRes.Value.OccupancyRate.Should().Be(50.0m);
    }

    [Fact]
    public async Task TransferBed_ShouldUpdateBothBedsAndRecordTransferHistory()
    {
        using var db = CreateDbContext(nameof(TransferBed_ShouldUpdateBothBedsAndRecordTransferHistory));
        var (_, bed1, bed2) = await SeedRoomAndBeds(db);

        var admHandler = new CreateAdmissionHandler(db);
        var adm = (await admHandler.Handle(new CreateAdmissionCommand(
            Guid.NewGuid(), "BN102", "Lê Z", null, "Male", null, null,
            "KHOA_NOI", "Khoa Nội", bed1.Id, "BS. Nam", "Hen phế quản", "J45", "Hen phế quản", 0m
        ), CancellationToken.None)).Value!;

        var transferHandler = new TransferBedHandler(db);
        var res = await transferHandler.Handle(new TransferBedCommand(
            adm.Id, bed2.Id, "Chuyển sang giường thoáng hơn", "ĐD. Hà"
        ), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();

        var oldBed = await db.Beds.FindAsync(bed1.Id);
        var newBed = await db.Beds.FindAsync(bed2.Id);

        oldBed!.Status.Should().Be(BedStatus.Cleaning);
        oldBed.CurrentAdmissionId.Should().BeNull();

        newBed!.Status.Should().Be(BedStatus.Occupied);
        newBed.CurrentAdmissionId.Should().Be(adm.Id);

        var history = await db.TransferHistories.FirstOrDefaultAsync(h => h.AdmissionId == adm.Id);
        history.Should().NotBeNull();
        history!.FromBedCode.Should().Be("G-101-01");
        history.ToBedCode.Should().Be("G-101-02");
    }

    [Fact]
    public async Task ClinicalHandlers_ShouldRecordOrderVitalSignsAndCareNotes()
    {
        using var db = CreateDbContext(nameof(ClinicalHandlers_ShouldRecordOrderVitalSignsAndCareNotes));
        var (_, bed1, _) = await SeedRoomAndBeds(db);

        var adm = (await new CreateAdmissionHandler(db).Handle(new CreateAdmissionCommand(
            Guid.NewGuid(), "BN103", "Phạm W", null, "Male", null, null,
            "KHOA_NOI", "Khoa Nội", bed1.Id, "BS. Nam", "Viêm tụy", "K85", "Viêm tụy", 0m
        ), CancellationToken.None)).Value!;

        // 1. Y lệnh
        var orderHandler = new CreateMedicalOrderHandler(db);
        var orderRes = await orderHandler.Handle(new CreateMedicalOrderCommand(
            adm.Id, "BS. Cường", "Viêm tụy cấp", CareLevel.Level1, "Nhịn ăn uống",
            "NaCl 0.9% 500ml x 2 chai TTM 40g/p", "Amylase máu", "Theo dõi đau bụng"
        ), CancellationToken.None);
        orderRes.IsSuccess.Should().BeTrue();

        // 2. Sinh hiệu
        var vitalHandler = new RecordVitalSignHandler(db);
        var vitalRes = await vitalHandler.Handle(new RecordVitalSignCommand(
            adm.Id, 84, 37.2m, 120, 80, 20, 98, 65, "Bình thường", "ĐD. Mai"
        ), CancellationToken.None);
        vitalRes.IsSuccess.Should().BeTrue();

        // 3. Phiếu chăm sóc
        var careHandler = new CreateCareNoteHandler(db);
        var careRes = await careHandler.Handle(new CreateCareNoteCommand(
            adm.Id, "Bệnh nhân tỉnh, còn đau bụng thượng vị", "Truyền dịch theo y lệnh", "Đỡ đau", "ĐD. Mai"
        ), CancellationToken.None);
        careRes.IsSuccess.Should().BeTrue();

        // 4. Tạm ứng
        var advanceHandler = new RecordAdvancePaymentHandler(db);
        var advanceRes = await advanceHandler.Handle(new RecordAdvancePaymentCommand(
            adm.Id, 3000000m, "TRANSFER", "Thu ngân 02", "Tạm ứng lần 2"
        ), CancellationToken.None);
        advanceRes.IsSuccess.Should().BeTrue();

        var admDetail = (await new GetAdmissionDetailHandler(db).Handle(new GetAdmissionDetailQuery(adm.Id), CancellationToken.None)).Value!;
        admDetail.MedicalOrders.Should().HaveCount(1);
        admDetail.VitalSigns.Should().HaveCount(1);
        admDetail.CareNotes.Should().HaveCount(1);
        admDetail.AdvancePayments.Should().HaveCount(1);
    }

    [Fact]
    public async Task DischargeAdmission_ShouldSetDischargedStatusAndReleaseBedToCleaning()
    {
        using var db = CreateDbContext(nameof(DischargeAdmission_ShouldSetDischargedStatusAndReleaseBedToCleaning));
        var (_, bed1, _) = await SeedRoomAndBeds(db);

        var adm = (await new CreateAdmissionHandler(db).Handle(new CreateAdmissionCommand(
            Guid.NewGuid(), "BN104", "Vũ Q", null, "Male", null, null,
            "KHOA_NOI", "Khoa Nội", bed1.Id, "BS. Nam", "Viêm ruột", "A09", "Viêm ruột", 0m
        ), CancellationToken.None)).Value!;

        var dischargeHandler = new DischargeAdmissionHandler(db);
        var res = await dischargeHandler.Handle(new DischargeAdmissionCommand(
            adm.Id, "Viêm ruột cấp đã ổn định", "A09", DischargeOutcome.Cured,
            "Khỏi bệnh, cho xuất viện nghỉ ngơi 3 ngày", "BS. Nam"
        ), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();

        var admAfter = await db.Admissions.FindAsync(adm.Id);
        admAfter!.Status.Should().Be(AdmissionStatus.Discharged);
        admAfter.Outcome.Should().Be(DischargeOutcome.Cured);

        var bedAfter = await db.Beds.FindAsync(bed1.Id);
        bedAfter!.Status.Should().Be(BedStatus.Cleaning);
        bedAfter.CurrentAdmissionId.Should().BeNull();
    }
}

