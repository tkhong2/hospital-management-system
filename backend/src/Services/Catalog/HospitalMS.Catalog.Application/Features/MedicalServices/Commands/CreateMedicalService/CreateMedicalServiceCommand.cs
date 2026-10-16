using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Domain.Entities;
using HospitalMS.Catalog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Application.Features.MedicalServices.Commands.CreateMedicalService;

public record CreateMedicalServiceCommand(
    string Code,
    string Name,
    ServiceType ServiceType,
    string? UnitCode,
    string? UnitName,
    decimal Price,
    decimal HealthInsurancePrice,
    Guid? DepartmentId,
    string? DepartmentName,
    string? Description
) : IRequest<Result<Guid>>;

public class CreateMedicalServiceCommandValidator : AbstractValidator<CreateMedicalServiceCommand>
{
    public CreateMedicalServiceCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HealthInsurancePrice).GreaterThanOrEqualTo(0);
    }
}

public class CreateMedicalServiceCommandHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<CreateMedicalServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMedicalServiceCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.MedicalServices
            .AnyAsync(s => s.Code.ToLower() == request.Code.ToLower() && !s.IsDeleted, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>($"Mã dịch vụ kỹ thuật {request.Code} đã tồn tại.");
        }

        var service = new MedicalService
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            ServiceType = request.ServiceType,
            UnitCode = request.UnitCode,
            UnitName = request.UnitName,
            Price = request.Price,
            HealthInsurancePrice = request.HealthInsurancePrice,
            DepartmentId = request.DepartmentId,
            DepartmentName = request.DepartmentName,
            Description = request.Description,
            IsActive = true
        };

        dbContext.MedicalServices.Add(service);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("catalog:services", cancellationToken);

        return Result.Success(service.Id);
    }
}

