using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Application.DTOs;
using HospitalMS.Catalog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Application.Features.MedicalServices.Queries.GetMedicalServices;

public record GetMedicalServicesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    ServiceType? ServiceType = null,
    Guid? DepartmentId = null,
    bool? IsActive = true
) : IRequest<Result<PagedResult<MedicalServiceDto>>>;

public class GetMedicalServicesQueryHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<GetMedicalServicesQuery, Result<PagedResult<MedicalServiceDto>>>
{
    public async Task<Result<PagedResult<MedicalServiceDto>>> Handle(GetMedicalServicesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"catalog:services:p{request.PageNumber}:s{request.PageSize}:t{request.ServiceType}:d{request.DepartmentId}:q{request.Search}:a{request.IsActive}";

        var cached = await cacheService.GetAsync<PagedResult<MedicalServiceDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var query = dbContext.MedicalServices
            .Where(s => !s.IsDeleted)
            .AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        if (request.ServiceType.HasValue)
        {
            query = query.Where(s => s.ServiceType == request.ServiceType.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(s => s.DepartmentId == request.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(search) ||
                s.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.ServiceType)
            .ThenBy(s => s.Code)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new MedicalServiceDto(
                s.Id,
                s.Code,
                s.Name,
                s.ServiceType,
                s.UnitCode,
                s.UnitName,
                s.Price,
                s.HealthInsurancePrice,
                s.DepartmentId,
                s.DepartmentName,
                s.Description,
                s.IsActive
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<MedicalServiceDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize
        );

        // Cache for 10 minutes
        await cacheService.SetAsync(cacheKey, pagedResult, TimeSpan.FromMinutes(10), cancellationToken);

        return Result.Success(pagedResult);
    }
}

