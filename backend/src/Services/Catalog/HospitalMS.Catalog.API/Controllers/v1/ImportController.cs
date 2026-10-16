using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Features.Import.Commands;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Catalog.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ImportController : BaseApiController
{
    /// <summary>
    /// Import hàng loạt danh mục dịch vụ kỹ thuật từ Excel/CSV/JSON.
    /// Tự động cập nhật nếu đã tồn tại mã dịch vụ hoặc thêm mới nếu chưa có.
    /// </summary>
    [HttpPost("medical-services")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportMedicalServices(
        [FromBody] ImportMedicalServicesCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Import hàng loạt danh mục mã bệnh ICD-10 từ Excel/CSV/JSON.
    /// </summary>
    [HttpPost("icd10")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportIcd10(
        [FromBody] ImportIcd10Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

