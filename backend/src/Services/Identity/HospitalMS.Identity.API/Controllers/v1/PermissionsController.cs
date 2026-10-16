using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Features.Permissions.Queries.GetPermissions;
using HospitalMS.Identity.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Identity.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PermissionsController : BaseApiController
{
    /// <summary>
    /// Lấy danh sách toàn bộ các quyền hạn phân nhóm theo Module hệ thống.
    /// </summary>
    [HttpGet]
    [HasPermission("roles.view")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissionGroupDto>>>> GetPermissions()
    {
        var result = await Mediator.Send(new GetPermissionsQuery());
        return HandleResult(result);
    }
}

