using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Domain.Enums;

namespace HospitalMS.Identity.Domain.Entities;

public class User : AggregateRoot
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public List<Role> Roles { get; set; } = [];
    public List<string> AccessibleApps { get; set; } = [];
}
