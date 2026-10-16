namespace HospitalMS.Identity.Domain.Entities;

public class RoleApplication
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
}

