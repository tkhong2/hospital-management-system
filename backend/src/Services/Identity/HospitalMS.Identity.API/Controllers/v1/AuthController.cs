using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Features.Auth.Commands.Login;
using HospitalMS.Identity.Application.Features.Auth.Queries.GetAppLauncher;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Identity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseApiController
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("apps")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AppItemDto>>>> GetAccessibleApps([FromQuery] string? username)
    {
        var result = await Mediator.Send(new GetAppLauncherQuery(username ?? "admin"));
        return HandleResult(result);
    }
}
