using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Features.Roles.Commands.CreateRole;
using HospitalMS.Identity.Application.Features.Roles.Commands.DeleteRole;
using HospitalMS.Identity.Application.Features.Roles.Commands.UpdateRole;
using HospitalMS.Identity.Application.Features.Roles.Queries.GetRoles;
using HospitalMS.Identity.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Identity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RolesController : BaseApiController
{
    /// <summary>
    /// Lấy danh sách toàn bộ các vai trò và quyền hạn tương ứng.
    /// </summary>
    [HttpGet]
    [HasPermission("roles.view")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> GetRoles()
    {
        var result = await Mediator.Send(new GetRolesQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo vai trò mới với danh sách quyền và ứng dụng được phép truy cập.
    /// </summary>
    [HttpPost]
    [HasPermission("roles.create")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật tên, mô tả, danh sách quyền và ứng dụng của vai trò.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission("roles.update")]
    public async Task<ActionResult<ApiResponse>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(
            Id: id,
            Name: request.Name,
            Description: request.Description,
            PermissionIds: request.PermissionIds,
            ApplicationIds: request.ApplicationIds
        );
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa vai trò tùy chỉnh.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("roles.delete")]
    public async Task<ActionResult<ApiResponse>> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand(id);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record UpdateRoleRequest(
    string Name,
    string? Description,
    List<Guid> PermissionIds,
    List<Guid> ApplicationIds
);

