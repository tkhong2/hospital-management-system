using FluentValidation;
using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Domain.Entities;
using HospitalMS.Patient.Domain.Enums;
using HospitalMS.Patient.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand(
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
) : IRequest<Result<Guid>>;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên bệnh nhân không được để trống.")
            .MaximumLength(150).WithMessage("Họ và tên tối đa 150 ký tự.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Ngày sinh không được lớn hơn thời điểm hiện tại.");

        RuleFor(x => x.IdentityCardNumber)
            .Matches(@"^[0-9]{9,12}$").When(x => !string.IsNullOrEmpty(x.IdentityCardNumber))
            .WithMessage("Số CCCD/CMND phải gồm 9 đến 12 chữ số.");

        RuleFor(x => x.HealthInsuranceNumber)
            .Length(10, 15).When(x => !string.IsNullOrEmpty(x.HealthInsuranceNumber))
            .WithMessage("Mã số thẻ BHYT hợp lệ từ 10 đến 15 ký tự.");
    }
}

public class CreatePatientCommandHandler(
    IPatientDbContext dbContext,
    IPatientCodeGenerator codeGenerator,
    IEventBus eventBus)
    : IRequestHandler<CreatePatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        // 1. Chống trùng lặp theo CCCD
        if (!string.IsNullOrWhiteSpace(request.IdentityCardNumber))
        {
            var cccd = request.IdentityCardNumber.Trim();
            var exists = await dbContext.Patients
                .AnyAsync(p => p.IdentityCardNumber == cccd && !p.IsDeleted && !p.IsMerged, cancellationToken);

            if (exists)
            {
                return Result.Failure<Guid>($"Đã tồn tại hồ sơ bệnh nhân với số CCCD/CMND {cccd}.");
            }
        }

        // 2. Chống trùng lặp theo BHYT
        if (!string.IsNullOrWhiteSpace(request.HealthInsuranceNumber))
        {
            var bhyt = request.HealthInsuranceNumber.Trim().ToUpper();
            var exists = await dbContext.Patients
                .AnyAsync(p => p.HealthInsuranceNumber != null && p.HealthInsuranceNumber.ToUpper() == bhyt && !p.IsDeleted && !p.IsMerged, cancellationToken);

            if (exists)
            {
                return Result.Failure<Guid>($"Đã tồn tại hồ sơ bệnh nhân với mã số BHYT {bhyt}.");
            }
        }

        // 3. Tự động sinh mã bệnh nhân chuẩn MPI
        var patientCode = await codeGenerator.GenerateCodeAsync(cancellationToken);

        var patient = new Domain.Entities.Patient
        {
            PatientCode = patientCode,
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            IdentityCardNumber = request.IdentityCardNumber?.Trim(),
            HealthInsuranceNumber = request.HealthInsuranceNumber?.Trim().ToUpper(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = request.Email?.Trim().ToLower(),
            DetailAddress = request.DetailAddress?.Trim(),
            WardCode = request.WardCode,
            WardName = request.WardName,
            DistrictCode = request.DistrictCode,
            DistrictName = request.DistrictName,
            ProvinceCode = request.ProvinceCode,
            ProvinceName = request.ProvinceName,
            Ethnic = request.Ethnic,
            Nationality = request.Nationality,
            Career = request.Career,
            BloodType = request.BloodType,
            Allergies = request.Allergies,
            Note = request.Note
        };

        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 4. Publish Event cho các phân hệ khác (Khám bệnh, Khám sức khỏe, Kho dược)
        var integrationEvent = new PatientCreatedIntegrationEvent(
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

        return Result.Success(patient.Id);
    }
}

