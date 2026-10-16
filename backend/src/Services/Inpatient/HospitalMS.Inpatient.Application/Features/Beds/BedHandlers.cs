using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Inpatient.Application.Common.Interfaces;
using HospitalMS.Inpatient.Application.DTOs;
using HospitalMS.Inpatient.Domain.Entities;
using HospitalMS.Inpatient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Inpatient.Application.Features.Beds;

public record GetBedMapQuery(string? DepartmentCode = null) : IRequest<Result<BedMapSummaryDto>>;

public class GetBedMapHandler(IInpatientDbContext dbContext)
    : IRequestHandler<GetBedMapQuery, Result<BedMapSummaryDto>>
{
    public async Task<Result<BedMapSummaryDto>> Handle(GetBedMapQuery request, CancellationToken cancellationToken)
    {
        var roomQuery = dbContext.Rooms
            .Include(r => r.Beds)
                .ThenInclude(b => b.CurrentAdmission)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            roomQuery = roomQuery.Where(r => r.DepartmentCode == request.DepartmentCode);
        }

        var rooms = await roomQuery.OrderBy(r => r.Floor).ThenBy(r => r.RoomNumber).ToListAsync(cancellationToken);

        var allBeds = rooms.SelectMany(r => r.Beds).ToList();
        var totalBeds = allBeds.Count;
        var emptyBeds = allBeds.Count(b => b.Status == BedStatus.Empty);
        var occupiedBeds = allBeds.Count(b => b.Status == BedStatus.Occupied);
        var cleaningBeds = allBeds.Count(b => b.Status == BedStatus.Cleaning);
        var maintenanceBeds = allBeds.Count(b => b.Status == BedStatus.Maintenance);
        var rate = totalBeds > 0 ? Math.Round((decimal)occupiedBeds / totalBeds * 100, 1) : 0m;

        var roomDtos = rooms.Select(r => new DepartmentRoomDto(
            r.Id,
            r.DepartmentCode,
            r.DepartmentName,
            r.RoomNumber,
            r.RoomName,
            r.Floor,
            r.Notes,
            r.Beds.OrderBy(b => b.BedCode).Select(b => new HospitalBedDto(
                b.Id,
                b.RoomId,
                r.RoomNumber,
                r.RoomName,
                r.DepartmentName,
                b.BedCode,
                b.BedName,
                b.BedType,
                b.Status,
                b.CurrentAdmissionId,
                b.CurrentAdmission?.PatientCode,
                b.CurrentAdmission?.PatientName,
                b.CurrentAdmission?.InitialDiagnosis,
                b.CurrentAdmission?.AdmissionTime,
                b.DailyPrice,
                b.InsuranceCoveredPrice
            )).ToList()
        )).ToList();

        var summary = new BedMapSummaryDto(
            totalBeds,
            emptyBeds,
            occupiedBeds,
            cleaningBeds,
            maintenanceBeds,
            rate,
            roomDtos
        );

        return Result.Success(summary);
    }
}

public record TransferBedCommand(
    Guid AdmissionId,
    Guid TargetBedId,
    string Reason,
    string? TransferredBy
) : IRequest<Result<bool>>;

public class TransferBedHandler(IInpatientDbContext dbContext)
    : IRequestHandler<TransferBedCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(TransferBedCommand request, CancellationToken cancellationToken)
    {
        var admission = await dbContext.Admissions.FirstOrDefaultAsync(a => a.Id == request.AdmissionId, cancellationToken);
        if (admission == null)
            return Result.Failure<bool>("Không tìm thấy hồ sơ bệnh án nội trú");

        if (admission.Status != AdmissionStatus.Admitted && admission.Status != AdmissionStatus.Transferring)
            return Result.Failure<bool>("Bệnh nhân không ở trạng thái có thể chuyển giường");

        var targetBed = await dbContext.Beds
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == request.TargetBedId, cancellationToken);

        if (targetBed == null)
            return Result.Failure<bool>("Không tìm thấy giường đích");

        if (targetBed.Status != BedStatus.Empty)
            return Result.Failure<bool>($"Giường đích {targetBed.BedCode} không trống (Trạng thái hiện tại: {targetBed.Status})");

        var oldBedId = admission.CurrentBedId;
        var oldBedCode = admission.CurrentBedCode;
        string? oldDeptName = admission.DepartmentName;

        // Giải phóng giường cũ (chuyển sang dọn vệ sinh)
        if (oldBedId.HasValue)
        {
            var oldBed = await dbContext.Beds.FirstOrDefaultAsync(b => b.Id == oldBedId.Value, cancellationToken);
            if (oldBed != null)
            {
                oldBed.Status = BedStatus.Cleaning;
                oldBed.CurrentAdmissionId = null;
            }
        }

        // Cập nhật giường mới
        targetBed.Status = BedStatus.Occupied;
        targetBed.CurrentAdmissionId = admission.Id;

        admission.CurrentBedId = targetBed.Id;
        admission.CurrentBedCode = targetBed.BedCode;
        admission.DepartmentCode = targetBed.Room?.DepartmentCode ?? admission.DepartmentCode;
        admission.DepartmentName = targetBed.Room?.DepartmentName ?? admission.DepartmentName;

        // Ghi nhận lịch sử chuyển giường
        var history = new BedTransferHistory
        {
            AdmissionId = admission.Id,
            FromBedId = oldBedId,
            FromBedCode = oldBedCode,
            ToBedId = targetBed.Id,
            ToBedCode = targetBed.BedCode,
            FromDepartmentName = oldDeptName,
            ToDepartmentName = targetBed.Room?.DepartmentName ?? "Khoa điều trị",
            TransferTime = DateTime.UtcNow,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Chuyển theo yêu cầu điều trị" : request.Reason,
            TransferredBy = request.TransferredBy ?? "Bác sĩ điều trị"
        };

        dbContext.TransferHistories.Add(history);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}

public record UpdateBedStatusCommand(
    Guid BedId,
    BedStatus NewStatus,
    string? Notes = null
) : IRequest<Result<bool>>;

public class UpdateBedStatusHandler(IInpatientDbContext dbContext)
    : IRequestHandler<UpdateBedStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateBedStatusCommand request, CancellationToken cancellationToken)
    {
        var bed = await dbContext.Beds.FirstOrDefaultAsync(b => b.Id == request.BedId, cancellationToken);
        if (bed == null)
            return Result.Failure<bool>("Không tìm thấy giường bệnh");

        if (bed.Status == BedStatus.Occupied && request.NewStatus != BedStatus.Occupied)
        {
            return Result.Failure<bool>("Giường đang có bệnh nhân, vui lòng thực hiện chuyển giường hoặc xuất viện trước");
        }

        bed.Status = request.NewStatus;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            bed.Notes = request.Notes;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}

