using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.DTOs;
using HospitalMS.Catalog.Application.Features.Icd10.Queries.GetIcd10;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Catalog.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class Icd10Controller : BaseApiController
{
    /// <summary>
    /// Tìm kiếm danh mục mã bệnh ICD-10 theo mã bệnh hoặc tên bệnh tiếng Việt / tiếng Anh.
    /// Có kết hợp cache Redis.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Icd10Dto>>>> GetIcd10(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? chapterCode = null)
    {
        var query = new GetIcd10Query(pageNumber, pageSize, search, chapterCode);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
}

