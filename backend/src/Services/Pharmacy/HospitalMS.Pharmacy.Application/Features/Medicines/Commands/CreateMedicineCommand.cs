using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Pharmacy.Application.Features.Medicines.Commands;

public record CreateMedicineCommand(
    string Code,
    string Name,
    string? RegistrationNumber,
    string ActiveIngredient,
    string Concentration,
    string UnitName,
    string? PackageSpecification,
    string? RouteOfAdministration,
    string? MedicineGroup,
    MedicineType MedicineType,
    SpecialControlType SpecialControl,
    decimal InboundPrice,
    decimal SalePrice,
    decimal? InsurancePrice,
    decimal MinStockLevel,
    decimal MaxStockLevel
) : IRequest<Result<Guid>>;

public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Mã thuốc không được để trống");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên thuốc không được để trống");
        RuleFor(x => x.ActiveIngredient).NotEmpty().WithMessage("Hoạt chất không được để trống");
        RuleFor(x => x.UnitName).NotEmpty().WithMessage("Đơn vị tính không được để trống");
        RuleFor(x => x.InboundPrice).GreaterThanOrEqualTo(0).WithMessage("Giá nhập không hợp lệ");
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).WithMessage("Giá bán không hợp lệ");
    }
}

public class CreateMedicineCommandHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<CreateMedicineCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Medicines
            .AnyAsync(x => x.Code == request.Code.Trim().ToUpper(), cancellationToken);

        if (existing)
        {
            return Result.Failure<Guid>($"Mã thuốc '{request.Code}' đã tồn tại trong danh mục");
        }

        var medicine = new Medicine
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            ActiveIngredient = request.ActiveIngredient.Trim(),
            Concentration = request.Concentration.Trim(),
            UnitName = request.UnitName.Trim(),
            PackageSpecification = request.PackageSpecification?.Trim(),
            RouteOfAdministration = request.RouteOfAdministration?.Trim(),
            MedicineGroup = request.MedicineGroup?.Trim(),
            MedicineType = request.MedicineType,
            SpecialControl = request.SpecialControl,
            InboundPrice = request.InboundPrice,
            SalePrice = request.SalePrice,
            InsurancePrice = request.InsurancePrice,
            MinStockLevel = request.MinStockLevel,
            MaxStockLevel = request.MaxStockLevel,
            IsActive = true
        };

        dbContext.Medicines.Add(medicine);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(medicine.Id);
    }
}

