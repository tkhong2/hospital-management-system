using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Medicines.Queries;

public record GetMedicinesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    MedicineType? MedicineType = null,
    SpecialControlType? SpecialControl = null,
    bool? IsActive = true
) : IRequest<Result<PagedResult<MedicineDto>>>;

public class GetMedicinesQueryHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<GetMedicinesQuery, Result<PagedResult<MedicineDto>>>
{
    public async Task<Result<PagedResult<MedicineDto>>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Medicines.AsNoTracking().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.MedicineType.HasValue)
        {
            query = query.Where(x => x.MedicineType == request.MedicineType.Value);
        }

        if (request.SpecialControl.HasValue)
        {
            query = query.Where(x => x.SpecialControl == request.SpecialControl.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.ActiveIngredient.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new MedicineDto(
                x.Id,
                x.Code,
                x.Name,
                x.RegistrationNumber,
                x.ActiveIngredient,
                x.Concentration,
                x.UnitName,
                x.PackageSpecification,
                x.RouteOfAdministration,
                x.MedicineGroup,
                x.MedicineType,
                x.SpecialControl,
                x.InboundPrice,
                x.SalePrice,
                x.InsurancePrice,
                x.MinStockLevel,
                x.MaxStockLevel,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<MedicineDto>(items, totalCount, request.PageNumber, request.PageSize);
        return Result.Success(pagedResult);
    }
}

