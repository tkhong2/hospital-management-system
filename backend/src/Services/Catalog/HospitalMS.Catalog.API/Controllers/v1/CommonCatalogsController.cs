using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.DTOs;
using HospitalMS.Catalog.Application.Features.CommonCatalogs.Queries;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Catalog.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class CommonCatalogsController : BaseApiController
{
    /// <summary>
    /// Lấy danh mục Khoa/Phòng (Lâm sàng, Cận lâm sàng, Hành chính).
    /// </summary>
    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> GetDepartments([FromQuery] string? type = null)
    {
        var result = await Mediator.Send(new GetDepartmentsQuery(type));
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh mục Chuyên khoa (Nội, Ngoại, Mắt, TMH, RHM, Da liễu, Sản phụ khoa...).
    /// </summary>
    [HttpGet("specialties")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SpecialtyDto>>>> GetSpecialties()
    {
        var result = await Mediator.Send(new GetSpecialtiesQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh mục Đơn vị tính (Lần, Ca, Xét nghiệm, Viên...).
    /// </summary>
    [HttpGet("units")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UnitDto>>>> GetUnits()
    {
        var result = await Mediator.Send(new GetUnitsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh mục Địa bàn hành chính (Tỉnh/Thành phố, Quận/Huyện, Phường/Xã).
    /// </summary>
    [HttpGet("administrative-divisions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdministrativeDivisionDto>>>> GetAdministrativeDivisions(
        [FromQuery] string? level = null,
        [FromQuery] string? parentCode = null)
    {
        var result = await Mediator.Send(new GetAdministrativeDivisionsQuery(level, parentCode));
        return HandleResult(result);
    }
}

