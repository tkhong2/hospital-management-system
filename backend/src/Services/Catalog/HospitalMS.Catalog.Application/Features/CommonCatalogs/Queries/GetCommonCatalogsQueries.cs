using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Application.Features.CommonCatalogs.Queries;

// 1. Departments Query
public record GetDepartmentsQuery(string? Type = null) : IRequest<Result<IReadOnlyList<DepartmentDto>>>;

public class GetDepartmentsQueryHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<GetDepartmentsQuery, Result<IReadOnlyList<DepartmentDto>>>
{
    public async Task<Result<IReadOnlyList<DepartmentDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"catalog:departments:{request.Type ?? "all"}";
        var cached = await cacheService.GetAsync<IReadOnlyList<DepartmentDto>>(cacheKey, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        var query = dbContext.Departments
            .Where(d => d.IsActive && !d.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(d => d.Type == request.Type);
        }

        var list = await query
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Code, d.Name, d.Type, d.PhoneNumber, d.Location, d.IsActive))
            .ToListAsync(cancellationToken);

        await cacheService.SetAsync(cacheKey, list, TimeSpan.FromHours(1), cancellationToken);
        return Result.Success<IReadOnlyList<DepartmentDto>>(list);
    }
}

// 2. Specialties Query
public record GetSpecialtiesQuery : IRequest<Result<IReadOnlyList<SpecialtyDto>>>;

public class GetSpecialtiesQueryHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<GetSpecialtiesQuery, Result<IReadOnlyList<SpecialtyDto>>>
{
    public async Task<Result<IReadOnlyList<SpecialtyDto>>> Handle(GetSpecialtiesQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "catalog:specialties";
        var cached = await cacheService.GetAsync<IReadOnlyList<SpecialtyDto>>(cacheKey, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        var list = await dbContext.Specialties
            .Where(s => s.IsActive && !s.IsDeleted)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyDto(s.Id, s.Code, s.Name, s.Description, s.IsActive))
            .ToListAsync(cancellationToken);

        await cacheService.SetAsync(cacheKey, list, TimeSpan.FromHours(1), cancellationToken);
        return Result.Success<IReadOnlyList<SpecialtyDto>>(list);
    }
}

// 3. Units Query
public record GetUnitsQuery : IRequest<Result<IReadOnlyList<UnitDto>>>;

public class GetUnitsQueryHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<GetUnitsQuery, Result<IReadOnlyList<UnitDto>>>
{
    public async Task<Result<IReadOnlyList<UnitDto>>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = "catalog:units";
        var cached = await cacheService.GetAsync<IReadOnlyList<UnitDto>>(cacheKey, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        var list = await dbContext.Units
            .Where(u => u.IsActive && !u.IsDeleted)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UnitDto(u.Id, u.Code, u.Name, u.Description, u.IsActive))
            .ToListAsync(cancellationToken);

        await cacheService.SetAsync(cacheKey, list, TimeSpan.FromHours(1), cancellationToken);
        return Result.Success<IReadOnlyList<UnitDto>>(list);
    }
}

// 4. Administrative Divisions Query
public record GetAdministrativeDivisionsQuery(
    string? Level = null,
    string? ParentCode = null
) : IRequest<Result<IReadOnlyList<AdministrativeDivisionDto>>>;

public class GetAdministrativeDivisionsQueryHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<GetAdministrativeDivisionsQuery, Result<IReadOnlyList<AdministrativeDivisionDto>>>
{
    public async Task<Result<IReadOnlyList<AdministrativeDivisionDto>>> Handle(
        GetAdministrativeDivisionsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"catalog:admin_div:{request.Level ?? "all"}:{request.ParentCode ?? "root"}";
        var cached = await cacheService.GetAsync<IReadOnlyList<AdministrativeDivisionDto>>(cacheKey, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        var query = dbContext.AdministrativeDivisions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Level))
        {
            query = query.Where(a => a.Level == request.Level);
        }

        if (!string.IsNullOrWhiteSpace(request.ParentCode))
        {
            query = query.Where(a => a.ParentCode == request.ParentCode);
        }

        var list = await query
            .OrderBy(a => a.Name)
            .Select(a => new AdministrativeDivisionDto(a.Id, a.Code, a.Name, a.Level, a.ParentCode))
            .ToListAsync(cancellationToken);

        await cacheService.SetAsync(cacheKey, list, TimeSpan.FromHours(2), cancellationToken);
        return Result.Success<IReadOnlyList<AdministrativeDivisionDto>>(list);
    }
}

