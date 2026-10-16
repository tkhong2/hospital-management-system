using HospitalMS.BuildingBlocks.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.BuildingBlocks.Common;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value!, traceId));
        }

        return BadRequest(ApiResponse<T>.Fail(result.Error ?? "An error occurred.", traceId));
    }

    protected ActionResult<ApiResponse<PagedResult<T>>> HandlePagedResult<T>(Result<PagedResult<T>> result)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<PagedResult<T>>.Ok(result.Value!, traceId));
        }

        return BadRequest(ApiResponse<PagedResult<T>>.Fail(result.Error ?? "An error occurred.", traceId));
    }

    protected ActionResult<ApiResponse> HandleResult(Result result)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.Ok(null, traceId));
        }

        return BadRequest(ApiResponse.Fail(result.Error ?? "An error occurred.", traceId));
    }
}
