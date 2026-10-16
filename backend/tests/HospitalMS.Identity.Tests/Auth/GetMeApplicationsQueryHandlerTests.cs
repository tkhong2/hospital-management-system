using FluentAssertions;
using HospitalMS.Identity.Application.Features.Auth.Queries.GetMeApplications;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using HospitalMS.Identity.Tests.Common;
using Xunit;
using AppEntity = HospitalMS.Identity.Domain.Entities.Application;

namespace HospitalMS.Identity.Tests.Auth;

public class GetMeApplicationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_AdminUser_ReturnsAllActiveApplications()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();

        var adminRole = new Role { Code = "Admin", Name = "Quản trị viên", IsSystemRole = true };
        dbContext.Roles.Add(adminRole);

        var app1 = new AppEntity { Code = "HEALTH_EXAM", Name = "Khám Sức Khỏe", Path = "/healthexam", IsActive = true };
        var app2 = new AppEntity { Code = "PHARMACY", Name = "Kho Dược", Path = "/pharmacy", IsActive = true };
        var app3 = new AppEntity { Code = "INACTIVE_APP", Name = "Ứng Dụng Ẩn", Path = "/hidden", IsActive = false };
        dbContext.Applications.AddRange(app1, app2, app3);

        var adminUser = new User
        {
            Username = "sysadmin",
            Email = "admin@hospital.vn",
            FullName = "Administrator",
            PasswordHash = "hash",
            Status = UserStatus.Active
        };
        adminUser.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });
        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync();

        var handler = new GetMeApplicationsQueryHandler(dbContext);
        var query = new GetMeApplicationsQuery(adminUser.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Roles.Should().Contain("Admin");
        // Must return all active applications, excluding inactive ones
        result.Value.Applications.Should().HaveCount(2);
        result.Value.Applications.Select(a => a.Code).Should().Contain(["HEALTH_EXAM", "PHARMACY"]);
    }

    [Fact]
    public async Task Handle_RegularUser_ReturnsOnlyPermittedApplications()
    {
        // Arrange
        using var dbContext = IdentityTestDbContextFactory.Create();

        var examRole = new Role { Code = "Doctor_HealthExam", Name = "Bác sĩ Khám Sức Khỏe" };
        var app1 = new AppEntity { Code = "HEALTH_EXAM", Name = "Khám Sức Khỏe", Path = "/healthexam", IsActive = true };
        var app2 = new AppEntity { Code = "PHARMACY", Name = "Kho Dược", Path = "/pharmacy", IsActive = true };

        examRole.RoleApplications.Add(new RoleApplication { Role = examRole, Application = app1 });

        dbContext.Roles.Add(examRole);
        dbContext.Applications.AddRange(app1, app2);

        var regularDoctor = new User
        {
            Username = "doctor_exam",
            Email = "doctorexam@hospital.vn",
            FullName = "Bác Sĩ Đoàn Khám",
            PasswordHash = "hash",
            Status = UserStatus.Active
        };
        regularDoctor.UserRoles.Add(new UserRole { User = regularDoctor, Role = examRole });
        dbContext.Users.Add(regularDoctor);
        await dbContext.SaveChangesAsync();

        var handler = new GetMeApplicationsQueryHandler(dbContext);
        var query = new GetMeApplicationsQuery(regularDoctor.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Applications.Should().HaveCount(1);
        result.Value.Applications.Single().Code.Should().Be("HEALTH_EXAM");
    }
}

