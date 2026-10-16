using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Features.Auth.Commands.ChangePassword;
using HospitalMS.Identity.Application.Features.Auth.Commands.Login;
using HospitalMS.Identity.Application.Features.Auth.Commands.Logout;
using HospitalMS.Identity.Application.Features.Auth.Commands.RefreshToken;
using HospitalMS.Identity.Application.Features.Auth.Queries.GetMeApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Identity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(ICurrentUser currentUser) : BaseApiController
{
    /// <summary>
    /// Đăng nhập hệ thống bằng username và password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new LoginCommand(request.Username, request.Password, ipAddress);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token (Token Rotation).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken, ipAddress);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Đăng xuất và thu hồi Refresh Token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new LogoutCommand(request.RefreshToken, ipAddress);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Đổi mật khẩu cho tài khoản hiện tại.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId) || !Guid.TryParse(currentUser.UserId, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Không xác định được danh tính người dùng."));
        }

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các ứng dụng được phép truy cập và quyền của người dùng hiện tại (App Launcher).
    /// </summary>
    [HttpGet("me/applications")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserApplicationsDto>>> GetMyApplications()
    {
        if (string.IsNullOrEmpty(currentUser.UserId) || !Guid.TryParse(currentUser.UserId, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Không xác định được danh tính người dùng."));
        }

        var result = await Mediator.Send(new GetMeApplicationsQuery(userId));
        return HandleResult(result);
    }
}

public record LoginRequest(string Username, string Password);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record LogoutRequest(string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
