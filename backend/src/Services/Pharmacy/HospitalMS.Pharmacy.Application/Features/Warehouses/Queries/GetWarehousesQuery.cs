using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery(bool? IsActive = true)
    : IRequest<Result<List<WarehouseDto>>>;

public class GetWarehousesQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetWarehousesQuery, Result<List<WarehouseDto>>>
{
    public async Task<Result<List<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Warehouses.AsNoTracking().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var items = await query
            .OrderBy(x => x.WarehouseType)
            .ThenBy(x => x.Name)
            .Select(x => new WarehouseDto(
                x.Id,
                x.Code,
                x.Name,
                x.WarehouseType,
                GetWarehouseTypeName(x.WarehouseType),
                x.DepartmentId,
                x.DepartmentName,
                x.Location,
                x.ManagerName,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }

    private static string GetWarehouseTypeName(WarehouseType type) => type switch
    {
        WarehouseType.MainWarehouse => "Kho chẵn / Kho trung tâm",
        WarehouseType.SubWarehouse => "Kho lẻ ngoại trú",
        WarehouseType.EmergencyCabinet => "Tủ trực cấp cứu khoa",
        WarehouseType.InpatientWarehouse => "Kho dược nội trú",
        _ => type.ToString()
    };
}

