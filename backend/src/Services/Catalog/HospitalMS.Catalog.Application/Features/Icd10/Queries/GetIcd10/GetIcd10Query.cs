using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Application.Features.Icd10.Queries.GetIcd10;

public record GetIcd10Query(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? ChapterCode = null
) : IRequest<Result<PagedResult<Icd10Dto>>>;

public class GetIcd10QueryHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<GetIcd10Query, Result<PagedResult<Icd10Dto>>>
{
    public async Task<Result<PagedResult<Icd10Dto>>> Handle(GetIcd10Query request, CancellationToken cancellationToken)
    {
        var cacheKey = $"catalog:icd10:p{request.PageNumber}:s{request.PageSize}:c{request.ChapterCode}:q{request.Search}";

        var cached = await cacheService.GetAsync<PagedResult<Icd10Dto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var query = dbContext.Icd10s
            .Where(i => i.IsActive && !i.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.ChapterCode))
        {
            query = query.Where(i => i.ChapterCode == request.ChapterCode);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(i =>
                i.Code.ToLower().Contains(search) ||
                i.Name.ToLower().Contains(search) ||
                (i.NameEn != null && i.NameEn.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Code)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new Icd10Dto(
                i.Id,
                i.Code,
                i.Name,
                i.NameEn,
                i.ChapterCode,
                i.ChapterName,
                i.IsActive
            ))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<Icd10Dto>(
            items: items,
            totalCount: totalCount,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize
        );

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return Result.Success(result);
    }
}

