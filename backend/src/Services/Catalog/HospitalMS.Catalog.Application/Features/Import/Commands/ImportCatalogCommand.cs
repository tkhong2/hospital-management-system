using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Domain.Entities;
using HospitalMS.Catalog.Domain.Enums;
using Icd10Entity = HospitalMS.Catalog.Domain.Entities.Icd10;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Catalog.Application.Features.Import.Commands;

public record ImportMedicalServiceItem(
    string Code,
    string Name,
    ServiceType ServiceType,
    string? UnitName,
    decimal Price,
    decimal HealthInsurancePrice,
    string? DepartmentName,
    string? Description
);

public record ImportMedicalServicesCommand(
    List<ImportMedicalServiceItem> Items
) : IRequest<Result<ImportResultDto>>;

public record ImportIcd10Item(
    string Code,
    string Name,
    string? NameEn,
    string ChapterCode,
    string ChapterName
);

public record ImportIcd10Command(
    List<ImportIcd10Item> Items
) : IRequest<Result<ImportResultDto>>;

public record ImportResultDto(
    int TotalProcessed,
    int InsertedCount,
    int UpdatedCount,
    List<string> Errors
);

public class ImportCatalogCommandHandler(
    ICatalogDbContext dbContext,
    ICacheService cacheService)
    : IRequestHandler<ImportMedicalServicesCommand, Result<ImportResultDto>>,
      IRequestHandler<ImportIcd10Command, Result<ImportResultDto>>
{
    public async Task<Result<ImportResultDto>> Handle(
        ImportMedicalServicesCommand request,
        CancellationToken cancellationToken)
    {
        var inserted = 0;
        var updated = 0;
        var errors = new List<string>();

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Code) || string.IsNullOrWhiteSpace(item.Name))
            {
                errors.Add($"Dòng thiếu mã hoặc tên dịch vụ: {item.Code}");
                continue;
            }

            var existing = await dbContext.MedicalServices
                .FirstOrDefaultAsync(s => s.Code.ToLower() == item.Code.ToLower(), cancellationToken);

            if (existing is not null)
            {
                existing.Name = item.Name.Trim();
                existing.ServiceType = item.ServiceType;
                existing.UnitName = item.UnitName;
                existing.Price = item.Price;
                existing.HealthInsurancePrice = item.HealthInsurancePrice;
                existing.DepartmentName = item.DepartmentName;
                existing.Description = item.Description;
                existing.IsActive = true;
                updated++;
            }
            else
            {
                var newService = new MedicalService
                {
                    Code = item.Code.Trim().ToUpper(),
                    Name = item.Name.Trim(),
                    ServiceType = item.ServiceType,
                    UnitName = item.UnitName,
                    Price = item.Price,
                    HealthInsurancePrice = item.HealthInsurancePrice,
                    DepartmentName = item.DepartmentName,
                    Description = item.Description,
                    IsActive = true
                };
                dbContext.MedicalServices.Add(newService);
                inserted++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("catalog:services", cancellationToken);

        return Result.Success(new ImportResultDto(request.Items.Count, inserted, updated, errors));
    }

    public async Task<Result<ImportResultDto>> Handle(
        ImportIcd10Command request,
        CancellationToken cancellationToken)
    {
        var inserted = 0;
        var updated = 0;
        var errors = new List<string>();

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Code) || string.IsNullOrWhiteSpace(item.Name))
            {
                errors.Add($"Dòng thiếu mã hoặc tên ICD-10: {item.Code}");
                continue;
            }

            var existing = await dbContext.Icd10s
                .FirstOrDefaultAsync(i => i.Code.ToLower() == item.Code.ToLower(), cancellationToken);

            if (existing is not null)
            {
                existing.Name = item.Name.Trim();
                existing.NameEn = item.NameEn;
                existing.ChapterCode = item.ChapterCode;
                existing.ChapterName = item.ChapterName;
                existing.IsActive = true;
                updated++;
            }
            else
            {
                var newIcd = new Icd10Entity
                {
                    Code = item.Code.Trim().ToUpper(),
                    Name = item.Name.Trim(),
                    NameEn = item.NameEn,
                    ChapterCode = item.ChapterCode,
                    ChapterName = item.ChapterName,
                    IsActive = true
                };
                dbContext.Icd10s.Add(newIcd);
                inserted++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveByPrefixAsync("catalog:icd10", cancellationToken);

        return Result.Success(new ImportResultDto(request.Items.Count, inserted, updated, errors));
    }
}
