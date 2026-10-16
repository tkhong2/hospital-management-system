using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.DTOs;
using HospitalMS.Catalog.Application.Features.MedicalServices.Commands.CreateMedicalService;
using HospitalMS.Catalog.Application.Features.MedicalServices.Queries.GetMedicalServices;
using HospitalMS.Catalog.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Catalog.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class MedicalServicesController : BaseApiController
{
    /// <summary>
    /// Lấy danh mục dịch vụ kỹ thuật có phân trang, lọc theo nhóm (Khám, CLS, CĐHA, Thủ thuật) và tìm kiếm.
    /// Có kết hợp cache Redis.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MedicalServiceDto>>>> GetMedicalServices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] ServiceType? serviceType = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isActive = true)
    {
        var query = new GetMedicalServicesQuery(pageNumber, pageSize, search, serviceType, departmentId, isActive);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Thêm mới một dịch vụ kỹ thuật và xóa cache danh mục tương ứng.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMedicalService([FromBody] CreateMedicalServiceCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

