using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Application.Features.Patients.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<Result<PatientDto>>;

public class GetPatientByIdQueryHandler(IPatientDbContext dbContext)
    : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (patient is null)
        {
            return Result.Failure<PatientDto>("Không tìm thấy hồ sơ bệnh nhân.");
        }

        var dto = new PatientDto(
            patient.Id,
            patient.PatientCode,
            patient.FullName,
            patient.DateOfBirth,
            patient.Gender,
            patient.IdentityCardNumber,
            patient.HealthInsuranceNumber,
            patient.PhoneNumber,
            patient.Email,
            patient.DetailAddress,
            patient.WardCode,
            patient.WardName,
            patient.DistrictCode,
            patient.DistrictName,
            patient.ProvinceCode,
            patient.ProvinceName,
            patient.FullAddress,
            patient.Ethnic,
            patient.Nationality,
            patient.Career,
            patient.BloodType,
            patient.Allergies,
            patient.Note,
            patient.IsMerged,
            patient.MergedToPatientId,
            patient.CreatedAt
        );

        return Result.Success(dto);
    }
}

