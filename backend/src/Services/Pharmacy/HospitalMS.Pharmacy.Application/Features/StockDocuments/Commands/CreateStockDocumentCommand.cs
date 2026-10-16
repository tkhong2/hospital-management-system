using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Application.DTOs;
using HospitalMS.Pharmacy.Domain.Entities;
using HospitalMS.Pharmacy.Domain.Enums;
using MediatR;

namespace HospitalMS.Pharmacy.Application.Features.StockDocuments.Commands;

public record CreateStockDocumentCommand(
    DocumentType DocumentType,
    Guid? SourceWarehouseId,
    Guid? TargetWarehouseId,
    string? SupplierName,
    string? Note,
    List<StockDocumentItemDto> Details
) : IRequest<Result<Guid>>;

public class CreateStockDocumentCommandValidator : AbstractValidator<CreateStockDocumentCommand>
{
    public CreateStockDocumentCommandValidator()
    {
        RuleFor(x => x.Details).NotEmpty().WithMessage("Phiếu kho phải có ít nhất một mặt hàng");
        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.MedicineId).NotEmpty().WithMessage("Chưa chọn thuốc/vật tư");
            detail.RuleFor(d => d.Quantity).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
            detail.RuleFor(d => d.BatchNumber).NotEmpty().WithMessage("Số lô không được để trống");
        });
    }
}

public class CreateStockDocumentCommandHandler(IPharmacyDbContext dbContext)
    : IRequestHandler<CreateStockDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStockDocumentCommand request, CancellationToken cancellationToken)
    {
        var prefix = request.DocumentType switch
        {
            DocumentType.Inbound => "PN",
            DocumentType.Outbound => "PX",
            DocumentType.Transfer => "PC",
            DocumentType.Return => "PT",
            DocumentType.Dispose => "PH",
            _ => "PK"
        };

        var docNumber = $"{prefix}{DateTime.UtcNow:yyMM}{Random.Shared.Next(1000, 9999)}";
        var totalAmount = request.Details.Sum(x => x.Quantity * x.UnitPrice);

        var doc = new StockDocument
        {
            DocumentNumber = docNumber,
            DocumentType = request.DocumentType,
            SourceWarehouseId = request.SourceWarehouseId,
            TargetWarehouseId = request.TargetWarehouseId,
            SupplierName = request.SupplierName?.Trim(),
            DocumentDate = DateTime.UtcNow,
            Status = DocumentStatus.Draft,
            TotalAmount = totalAmount,
            Note = request.Note?.Trim()
        };

        foreach (var item in request.Details)
        {
            doc.Details.Add(new StockDocumentDetail
            {
                MedicineId = item.MedicineId,
                SourceBatchId = item.SourceBatchId,
                BatchNumber = item.BatchNumber.Trim().ToUpper(),
                ExpiryDate = item.ExpiryDate,
                ManufactureDate = item.ManufactureDate,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Note = item.Note?.Trim()
            });
        }

        dbContext.StockDocuments.Add(doc);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(doc.Id);
    }
}

