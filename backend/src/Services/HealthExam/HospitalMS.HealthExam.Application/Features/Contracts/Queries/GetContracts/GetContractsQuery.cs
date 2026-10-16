using HospitalMS.BuildingBlocks.Common;
using MediatR;

namespace HospitalMS.HealthExam.Application.Features.Contracts.Queries.GetContracts;

public record GetContractsQuery(int PageNumber = 1, int PageSize = 10, string? Search = null)
    : IRequest<Result<PagedResult<ContractDto>>>;

public record ContractDto(
    Guid Id,
    string ContractNumber,
    string CompanyName,
    string TaxCode,
    string RepresentativeName,
    string PhoneNumber,
    DateTime StartDate,
    DateTime EndDate,
    bool IsMobileExam,
    int EstimatedPatientCount,
    decimal TotalContractAmount,
    decimal TotalPaidAmount,
    decimal RemainingDebt,
    string Status
);

public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, Result<PagedResult<ContractDto>>>
{
    public Task<Result<PagedResult<ContractDto>>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
    {
        var sampleData = new List<ContractDto>
        {
            new(
                Guid.NewGuid(),
                "HĐ-2026/01-ABC",
                "Công ty Cổ phần Công nghệ ABC",
                "0102030405",
                "Nguyễn Văn A (Giám đốc)",
                "0901234567",
                DateTime.UtcNow.Date,
                DateTime.UtcNow.Date.AddDays(7),
                false,
                250,
                375_000_000m,
                150_000_000m,
                225_000_000m,
                "Đang khám"
            ),
            new(
                Guid.NewGuid(),
                "HĐ-2026/02-VNP",
                "Tập đoàn Dược phẩm Vinapharm",
                "0109988776",
                "Lê Thị B (Trưởng ban Nhân sự)",
                "0912345678",
                DateTime.UtcNow.Date.AddDays(2),
                DateTime.UtcNow.Date.AddDays(10),
                true,
                120,
                240_000_000m,
                240_000_000m,
                0m,
                "Chờ khám"
            ),
            new(
                Guid.NewGuid(),
                "HĐ-2026/03-LOG",
                "Công ty Vận tải & Logistics Miền Đông",
                "0301122334",
                "Phạm Quốc C (Giám đốc Điều hành)",
                "0987654321",
                DateTime.UtcNow.Date.AddDays(5),
                DateTime.UtcNow.Date.AddDays(12),
                false,
                85,
                102_000_000m,
                50_000_000m,
                52_000_000m,
                "Lên lịch"
            )
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            sampleData = sampleData.Where(x =>
                x.CompanyName.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                x.ContractNumber.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var pagedResult = new PagedResult<ContractDto>(
            items: sampleData,
            totalCount: sampleData.Count,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize
        );

        return Task.FromResult(Result.Success(pagedResult));
    }
}
