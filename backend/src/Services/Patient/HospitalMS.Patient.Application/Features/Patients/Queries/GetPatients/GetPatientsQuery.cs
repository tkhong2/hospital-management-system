using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Application.Features.Patients.DTOs;
using HospitalMS.Patient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    Gender? Gender = null,
    string? ProvinceCode = null,
    bool IncludeMerged = false
) : IRequest<Result<PagedResult<PatientListItemDto>>>;

public class GetPatientsQueryHandler(IPatientDbContext dbContext)
    : IRequestHandler<GetPatientsQuery, Result<PagedResult<PatientListItemDto>>>
{
    public async Task<Result<PagedResult<PatientListItemDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Patients
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (!request.IncludeMerged)
        {
            query = query.Where(p => !p.IsMerged);
        }

        if (request.Gender.HasValue)
        {
            query = query.Where(p => p.Gender == request.Gender.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ProvinceCode))
        {
            query = query.Where(p => p.ProvinceCode == request.ProvinceCode);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.PatientCode.ToLower().Contains(search) ||
                p.FullName.ToLower().Contains(search) ||
                (p.IdentityCardNumber != null && p.IdentityCardNumber.Contains(search)) ||
                (p.HealthInsuranceNumber != null && p.HealthInsuranceNumber.ToLower().Contains(search)) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientListItemDto(
                p.Id,
                p.PatientCode,
                p.FullName,
                p.DateOfBirth,
                p.Gender,
                p.IdentityCardNumber,
                p.HealthInsuranceNumber,
                p.PhoneNumber,
                p.FullAddress,
                p.BloodType,
                p.IsMerged,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<PatientListItemDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize
        ));
    }
}

