using HospitalMS.BuildingBlocks.Common;
using MediatR;

namespace HospitalMS.Identity.Application.Features.Auth.Queries.GetAppLauncher;

public record GetAppLauncherQuery(string Username) : IRequest<Result<IReadOnlyList<AppItemDto>>>;

public record AppItemDto(
    string Code,
    string Name,
    string Description,
    string Icon,
    string Path,
    string Color,
    string? Badge
);

public class GetAppLauncherQueryHandler : IRequestHandler<GetAppLauncherQuery, Result<IReadOnlyList<AppItemDto>>>
{
    public Task<Result<IReadOnlyList<AppItemDto>>> Handle(GetAppLauncherQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<AppItemDto> apps =
        [
            new("healthexam", "Khám sức khỏe", "Quản lý hợp đồng đoàn, gói khám, luồng trạm đa chuyên khoa, kết luận TT14/TT24", "HeartPulse", "/healthexam", "from-emerald-500 to-teal-600", "Trọng tâm"),
            new("outpatient", "Khám ngoại trú", "Tiếp đón, buồng khám chuyên khoa, chỉ định cận lâm sàng, kê đơn điện tử, viện phí", "Stethoscope", "/outpatient", "from-blue-500 to-indigo-600", null),
            new("inpatient", "Quản lý nội trú", "Nhập viện, sơ đồ giường bệnh trực quan, chuyển khoa, y lệnh điều trị, hồ sơ bệnh án", "BedDouble", "/inpatient", "from-violet-500 to-purple-600", null),
            new("pharmacy", "Kho dược & Vật tư", "Quản lý nhập xuất tồn, lô hạn dùng (FEFO/FIFO), duyệt đơn, cấp phát thuốc", "Pill", "/pharmacy", "from-amber-500 to-orange-600", null),
            new("patient", "Hồ sơ Bệnh nhân (MPI)", "Mã bệnh nhân duy nhất, CCCD, thẻ BHYT và lịch sử khám chữa bệnh toàn viện", "Users", "/patient", "from-cyan-500 to-blue-600", null),
            new("catalog", "Danh mục dùng chung", "ICD-10, dịch vụ kỹ thuật TT39, khoa phòng, phân loại thuốc và vật tư y tế", "Database", "/catalog", "from-slate-600 to-slate-800", null),
            new("admin", "Quản trị hệ thống", "Tài khoản người dùng, vai trò (Roles), ma trận quyền RBAC, giám sát nhật ký", "ShieldCheck", "/admin", "from-rose-500 to-red-600", null)
        ];

        return Task.FromResult(Result.Success(apps));
    }
}
