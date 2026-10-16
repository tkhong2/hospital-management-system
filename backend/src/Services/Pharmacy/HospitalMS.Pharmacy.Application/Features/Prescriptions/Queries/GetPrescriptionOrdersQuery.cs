using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Prescriptions.Queries;

public record GetPrescriptionOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    PrescriptionStatus? Status = null,
    PrescriptionSourceType? SourceType = null,
    Guid? WarehouseId = null,
    string? Search = null
) : IRequest<Result<PagedResult<PrescriptionOrderDto>>>;

public class GetPrescriptionOrdersQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetPrescriptionOrdersQuery, Result<PagedResult<PrescriptionOrderDto>>>
{
    public async Task<Result<PagedResult<PrescriptionOrderDto>>> Handle(GetPrescriptionOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.PrescriptionOrders
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Details)
                .ThenInclude(d => d.Medicine)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.SourceType.HasValue)
        {
            query = query.Where(x => x.SourceType == request.SourceType.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.PrescriptionCode.ToLower().Contains(search) ||
                x.PatientCode.ToLower().Contains(search) ||
                x.PatientName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new PrescriptionOrderDto(
                x.Id,
                x.PrescriptionCode,
                x.SourceType,
                GetSourceTypeName(x.SourceType),
                x.SourceReferenceId,
                x.PatientId,
                x.PatientCode,
                x.PatientName,
                x.DoctorName,
                x.Diagnosis,
                x.PrescribedAt,
                x.WarehouseId,
                x.Warehouse.Name,
                x.Status,
                GetStatusName(x.Status),
                x.TotalAmount,
                x.DispensedBy,
                x.DispensedAt,
                x.Note,
                x.Details.Select(d => new PrescriptionOrderDetailDto(
                    d.Id,
                    d.MedicineId,
                    d.Medicine.Code,
                    d.Medicine.Name,
                    d.UnitName,
                    d.QuantityRequested,
                    d.QuantityDispensed,
                    d.UnitPrice,
                    d.TotalPrice,
                    d.UsageInstructions,
                    d.AllocatedBatchId,
                    d.AllocatedBatchNumber,
                    d.AllocatedExpiryDate
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<PrescriptionOrderDto>(orders, totalCount, request.PageNumber, request.PageSize));
    }

    private static string GetSourceTypeName(PrescriptionSourceType type) => type switch
    {
        PrescriptionSourceType.Outpatient => "Khám ngoại trú",
        PrescriptionSourceType.Inpatient => "Nội trú / Buồng bệnh",
        PrescriptionSourceType.HealthExam => "Đoàn khám sức khỏe",
        _ => type.ToString()
    };

    private static string GetStatusName(PrescriptionStatus status) => status switch
    {
        PrescriptionStatus.Pending => "Chờ cấp phát",
        PrescriptionStatus.Approved => "Đã duyệt giữ chỗ",
        PrescriptionStatus.Dispensed => "Đã cấp phát",
        PrescriptionStatus.Cancelled => "Đã hủy đơn",
        _ => status.ToString()
    };
}

