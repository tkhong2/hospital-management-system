using FluentValidation;
using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Domain.Enums;
using HospitalMS.Patient.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand(
    Guid Id,
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber = null,
    string? HealthInsuranceNumber = null,
    string? PhoneNumber = null,
    string? Email = null,
    string? DetailAddress = null,
    string? WardCode = null,
    string? WardName = null,
    string? DistrictCode = null,
    string? DistrictName = null,
    string? ProvinceCode = null,
    string? ProvinceName = null,
    string? Ethnic = "Kinh",
    string? Nationality = "Việt Nam",
    string? Career = null,
    BloodType BloodType = BloodType.Unknown,
    string? Allergies = null,
    string? Note = null
) : IRequest<Result>;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DateOfBirth).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.IdentityCardNumber)
            .Matches(@"^[0-9]{9,12}$").When(x => !string.IsNullOrEmpty(x.IdentityCardNumber));
        RuleFor(x => x.HealthInsuranceNumber)
            .Length(10, 15).When(x => !string.IsNullOrEmpty(x.HealthInsuranceNumber));
    }
}

public class UpdatePatientCommandHandler(
    IPatientDbContext dbContext,
    IEventBus eventBus)
    : IRequestHandler<UpdatePatientCommand, Result>
{
    public async Task<Result> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (patient is null)
        {
            return Result.Failure("Không tìm thấy hồ sơ bệnh nhân.");
        }

        if (patient.IsMerged)
        {
            return Result.Failure("Hồ sơ bệnh nhân này đã được hợp nhất vào hồ sơ khác, không thể cập nhật.");
        }

        // Chống trùng CCCD với người khác
        if (!string.IsNullOrWhiteSpace(request.IdentityCardNumber))
        {
            var cccd = request.IdentityCardNumber.Trim();
            var exists = await dbContext.Patients
                .AnyAsync(p => p.IdentityCardNumber == cccd && p.Id != request.Id && !p.IsDeleted && !p.IsMerged, cancellationToken);

            if (exists)
            {
                return Result.Failure($"Số CCCD/CMND {cccd} đã thuộc về một bệnh nhân khác.");
            }
        }

        // Chống trùng BHYT với người khác
        if (!string.IsNullOrWhiteSpace(request.HealthInsuranceNumber))
        {
            var bhyt = request.HealthInsuranceNumber.Trim().ToUpper();
            var exists = await dbContext.Patients
                .AnyAsync(p => p.HealthInsuranceNumber != null && p.HealthInsuranceNumber.ToUpper() == bhyt && p.Id != request.Id && !p.IsDeleted && !p.IsMerged, cancellationToken);

            if (exists)
            {
                return Result.Failure($"Mã thẻ BHYT {bhyt} đã thuộc về một bệnh nhân khác.");
            }
        }

        patient.FullName = request.FullName.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.IdentityCardNumber = request.IdentityCardNumber?.Trim();
        patient.HealthInsuranceNumber = request.HealthInsuranceNumber?.Trim().ToUpper();
        patient.PhoneNumber = request.PhoneNumber?.Trim();
        patient.Email = request.Email?.Trim().ToLower();
        patient.DetailAddress = request.DetailAddress?.Trim();
        patient.WardCode = request.WardCode;
        patient.WardName = request.WardName;
        patient.DistrictCode = request.DistrictCode;
        patient.DistrictName = request.DistrictName;
        patient.ProvinceCode = request.ProvinceCode;
        patient.ProvinceName = request.ProvinceName;
        patient.Ethnic = request.Ethnic;
        patient.Nationality = request.Nationality;
        patient.Career = request.Career;
        patient.BloodType = request.BloodType;
        patient.Allergies = request.Allergies;
        patient.Note = request.Note;

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        var integrationEvent = new PatientUpdatedIntegrationEvent(
            patient.Id,
            patient.PatientCode,
            patient.FullName,
            patient.DateOfBirth,
            patient.Gender,
            patient.IdentityCardNumber,
            patient.HealthInsuranceNumber,
            patient.PhoneNumber,
            patient.FullAddress
        );

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        return Result.Success();
    }
}

