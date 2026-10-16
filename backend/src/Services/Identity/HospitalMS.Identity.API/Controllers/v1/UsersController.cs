using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Features.Users.Commands.CreateUser;
using HospitalMS.Identity.Application.Features.Users.Commands.DeleteUser;
using HospitalMS.Identity.Application.Features.Users.Commands.UpdateUser;
using HospitalMS.Identity.Application.Features.Users.Queries.GetUsers;
using HospitalMS.Identity.Domain.Enums;
using HospitalMS.Identity.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Identity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : BaseApiController
{
    /// <summary>
    /// Lấy danh sách người dùng có phân trang, lọc và tìm kiếm.
    /// </summary>
    [HttpGet]
    [HasPermission("users.view")]
    public async Task<ActionResult<ApiResponse<PagedResult<UserListItemDto>>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] UserStatus? status = null)
    {
        var query = new GetUsersQuery(pageNumber, pageSize, search, departmentId, status);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo người dùng mới trong hệ thống.
    /// </summary>
    [HttpPost]
    [HasPermission("users.create")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin và vai trò của người dùng.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission("users.update")]
    public async Task<ActionResult<ApiResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(
            Id: id,
            FullName: request.FullName,
            Email: request.Email,
            PhoneNumber: request.PhoneNumber,
            DepartmentId: request.DepartmentId,
            Status: request.Status,
            RoleIds: request.RoleIds
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa người dùng (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("users.delete")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record UpdateUserRequest(
    string FullName,
    string Email,
    string? PhoneNumber,
    Guid? DepartmentId,
    UserStatus Status,
    List<Guid> RoleIds
);

