using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Application.Features.Contracts.Queries.GetContracts;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.HealthExam.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ContractsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractDto>>>> GetContracts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await Mediator.Send(new GetContractsQuery(pageNumber, pageSize, search));
        return HandlePagedResult(result);
    }
}
