using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Features.Patients.Commands.CreatePatient;
using HospitalMS.Patient.Application.Features.Patients.Commands.DeletePatient;
using HospitalMS.Patient.Application.Features.Patients.Commands.MergePatients;
using HospitalMS.Patient.Application.Features.Patients.Commands.UpdatePatient;
using HospitalMS.Patient.Application.Features.Patients.DTOs;
using HospitalMS.Patient.Application.Features.Patients.Queries.CheckDuplicatePatient;
using HospitalMS.Patient.Application.Features.Patients.Queries.GetPatientById;
using HospitalMS.Patient.Application.Features.Patients.Queries.GetPatients;
using HospitalMS.Patient.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS.Patient.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class PatientsController : BaseApiController
{
    /// <summary>
    /// Tìm kiếm và phân trang danh sách hồ sơ bệnh nhân (MPI).
    /// Hỗ trợ tìm nhanh theo Mã bệnh nhân, CCCD, Mã thẻ BHYT, Số điện thoại hoặc Họ tên.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PatientListItemDto>>>> GetPatients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Gender? gender = null,
        [FromQuery] string? provinceCode = null,
        [FromQuery] bool includeMerged = false)
    {
        var query = new GetPatientsQuery(pageNumber, pageSize, search, gender, provinceCode, includeMerged);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một hồ sơ bệnh nhân theo Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PatientDto>>> GetPatientById(Guid id)
    {
        var result = await Mediator.Send(new GetPatientByIdQuery(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Tiếp đón và tạo mới hồ sơ bệnh nhân (tự sinh mã BN theo tháng/năm, kiểm tra trùng lặp).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreatePatient([FromBody] CreatePatientCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin hành chính, thẻ BHYT hoặc thông tin y tế của bệnh nhân.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> UpdatePatient(Guid id, [FromBody] UpdatePatientRequest request)
    {
        var command = new UpdatePatientCommand(
            Id: id,
            FullName: request.FullName,
            DateOfBirth: request.DateOfBirth,
            Gender: request.Gender,
            IdentityCardNumber: request.IdentityCardNumber,
            HealthInsuranceNumber: request.HealthInsuranceNumber,
            PhoneNumber: request.PhoneNumber,
            Email: request.Email,
            DetailAddress: request.DetailAddress,
            WardCode: request.WardCode,
            WardName: request.WardName,
            DistrictCode: request.DistrictCode,
            DistrictName: request.DistrictName,
            ProvinceCode: request.ProvinceCode,
            ProvinceName: request.ProvinceName,
            Ethnic: request.Ethnic,
            Nationality: request.Nationality,
            Career: request.Career,
            BloodType: request.BloodType,
            Allergies: request.Allergies,
            Note: request.Note
        );

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa hồ sơ bệnh nhân (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeletePatient(Guid id)
    {
        var result = await Mediator.Send(new DeletePatientCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Kiểm tra trùng lặp hồ sơ bệnh nhân trước khi tạo mới hoặc cập nhật.
    /// </summary>
    [HttpPost("check-duplicate")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DuplicateCandidateDto>>>> CheckDuplicate(
        [FromBody] CheckDuplicatePatientQuery query)
    {
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Hợp nhất hai hồ sơ bệnh nhân trùng lặp (Merge Source record into Target record).
    /// </summary>
    [HttpPost("merge")]
    public async Task<ActionResult<ApiResponse>> MergePatients([FromBody] MergePatientsCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record UpdatePatientRequest(
    string FullName,
    DateTime DateOfBirth,
    Gender Gender,
    string? IdentityCardNumber = null,
    string? HealthInsuranceNumber = null,
    string? PhoneNumber = null,
    string? Email = null,
    string? DetailAddress = null,
    string? WardCode = null,
    string? WardName = null,
    string? DistrictCode = null,
    string? DistrictName = null,
    string? ProvinceCode = null,
    string? ProvinceName = null,
    string? Ethnic = "Kinh",
    string? Nationality = "Việt Nam",
    string? Career = null,
    BloodType BloodType = BloodType.Unknown,
    string? Allergies = null,
    string? Note = null
);

