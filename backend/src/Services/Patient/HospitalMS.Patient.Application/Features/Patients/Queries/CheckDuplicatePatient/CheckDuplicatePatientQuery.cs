using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Application.Features.Patients.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Queries.CheckDuplicatePatient;

public record CheckDuplicatePatientQuery(
    string FullName,
    DateTime DateOfBirth,
    string? IdentityCardNumber = null,
    string? HealthInsuranceNumber = null,
    string? PhoneNumber = null,
    Guid? ExcludePatientId = null
) : IRequest<Result<IReadOnlyList<DuplicateCandidateDto>>>;

public class CheckDuplicatePatientQueryHandler(IPatientDbContext dbContext)
    : IRequestHandler<CheckDuplicatePatientQuery, Result<IReadOnlyList<DuplicateCandidateDto>>>
{
    public async Task<Result<IReadOnlyList<DuplicateCandidateDto>>> Handle(
        CheckDuplicatePatientQuery request,
        CancellationToken cancellationToken)
    {
        var candidates = new List<DuplicateCandidateDto>();

        var query = dbContext.Patients
            .Where(p => !p.IsDeleted && !p.IsMerged);

        if (request.ExcludePatientId.HasValue)
        {
            query = query.Where(p => p.Id != request.ExcludePatientId.Value);
        }

        // 1. Check CCCD / CMND
        if (!string.IsNullOrWhiteSpace(request.IdentityCardNumber))
        {
            var cccd = request.IdentityCardNumber.Trim();
            var matches = await query
                .Where(p => p.IdentityCardNumber == cccd)
                .Select(p => new DuplicateCandidateDto(
                    p.Id, p.PatientCode, p.FullName, p.DateOfBirth, p.Gender,
                    p.IdentityCardNumber, p.HealthInsuranceNumber, p.PhoneNumber,
                    "Trùng số CCCD / CMND"
                ))
                .ToListAsync(cancellationToken);

            candidates.AddRange(matches);
        }

        // 2. Check BHYT
        if (!string.IsNullOrWhiteSpace(request.HealthInsuranceNumber))
        {
            var bhyt = request.HealthInsuranceNumber.Trim().ToUpper();
            var matches = await query
                .Where(p => p.HealthInsuranceNumber != null && p.HealthInsuranceNumber.ToUpper() == bhyt)
                .Select(p => new DuplicateCandidateDto(
                    p.Id, p.PatientCode, p.FullName, p.DateOfBirth, p.Gender,
                    p.IdentityCardNumber, p.HealthInsuranceNumber, p.PhoneNumber,
                    "Trùng mã số thẻ BHYT"
                ))
                .ToListAsync(cancellationToken);

            candidates.AddRange(matches);
        }

        // 3. Check Name + DateOfBirth + PhoneNumber
        if (!string.IsNullOrWhiteSpace(request.FullName) && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var name = request.FullName.Trim().ToLower();
            var phone = request.PhoneNumber.Trim();
            var dobDate = request.DateOfBirth.Date;

            var matches = await query
                .Where(p => p.FullName.ToLower() == name &&
                            p.DateOfBirth.Date == dobDate &&
                            p.PhoneNumber == phone)
                .Select(p => new DuplicateCandidateDto(
                    p.Id, p.PatientCode, p.FullName, p.DateOfBirth, p.Gender,
                    p.IdentityCardNumber, p.HealthInsuranceNumber, p.PhoneNumber,
                    "Trùng Họ tên, Ngày sinh và Số điện thoại"
                ))
                .ToListAsync(cancellationToken);

            candidates.AddRange(matches);
        }

        // Distinct by Id
        var distinctCandidates = candidates
            .DistinctBy(c => c.Id)
            .ToList();

        return Result.Success<IReadOnlyList<DuplicateCandidateDto>>(distinctCandidates);
    }
}

