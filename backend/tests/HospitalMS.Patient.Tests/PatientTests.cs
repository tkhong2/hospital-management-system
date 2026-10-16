using FluentAssertions;
using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Application.Features.Patients.Commands.CreatePatient;
using HospitalMS.Patient.Application.Features.Patients.Commands.MergePatients;
using HospitalMS.Patient.Application.Features.Patients.Queries.CheckDuplicatePatient;
using HospitalMS.Patient.Domain.Entities;
using HospitalMS.Patient.Domain.Enums;
using HospitalMS.Patient.Domain.Events;
using HospitalMS.Patient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HospitalMS.Patient.Tests;

public class PatientTests
{
    private readonly Mock<IPatientCodeGenerator> _codeGeneratorMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    public PatientTests()
    {
        _codeGeneratorMock
            .Setup(c => c.GenerateCodeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("BN241099999");

        _currentUserMock.Setup(u => u.UserName).Returns("admin");
    }

    private PatientDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PatientDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new PatientDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CreatePatient_GeneratesPatientCodeAndPublishesEvent()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var handler = new CreatePatientCommandHandler(dbContext, _codeGeneratorMock.Object, _eventBusMock.Object);

        var command = new CreatePatientCommand(
            FullName: "Lê Văn Tiến",
            DateOfBirth: new DateTime(1988, 7, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender: Gender.Male,
            IdentityCardNumber: "001088019999",
            PhoneNumber: "0912999888"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var created = await dbContext.Patients.FindAsync(result.Value);
        created.Should().NotBeNull();
        created!.PatientCode.Should().Be("BN241099999");
        created.FullName.Should().Be("Lê Văn Tiến");

        _eventBusMock.Verify(e => e.PublishAsync(
            It.Is<PatientCreatedIntegrationEvent>(ev => ev.PatientCode == "BN241099999"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CreatePatient_WithDuplicateCCCD_ReturnsFailure()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        dbContext.Patients.Add(new Domain.Entities.Patient
        {
            PatientCode = "BN241000001",
            FullName = "Nguyễn Văn A",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            IdentityCardNumber = "001090012345"
        });
        await dbContext.SaveChangesAsync();

        var handler = new CreatePatientCommandHandler(dbContext, _codeGeneratorMock.Object, _eventBusMock.Object);
        var command = new CreatePatientCommand(
            FullName: "Nguyễn Văn A (Trùng)",
            DateOfBirth: DateTime.UtcNow.AddYears(-30),
            Gender: Gender.Male,
            IdentityCardNumber: "001090012345"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Đã tồn tại hồ sơ bệnh nhân với số CCCD/CMND 001090012345");
    }

    [Fact]
    public async Task CheckDuplicate_FindsMatchingCandidateByBHYT()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var existing = new Domain.Entities.Patient
        {
            PatientCode = "BN241000002",
            FullName = "Trần Thị B",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            HealthInsuranceNumber = "DN4010120894567"
        };
        dbContext.Patients.Add(existing);
        await dbContext.SaveChangesAsync();

        var handler = new CheckDuplicatePatientQueryHandler(dbContext);
        var query = new CheckDuplicatePatientQuery(
            FullName: "Trần Thị B",
            DateOfBirth: DateTime.UtcNow.AddYears(-25),
            HealthInsuranceNumber: "DN4010120894567"
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().ContainSingle();
        result.Value![0].Reason.Should().Contain("Trùng mã số thẻ BHYT");
        result.Value[0].PatientCode.Should().Be("BN241000002");
    }

    [Fact]
    public async Task MergePatients_MarksSourceMergedAndEnrichesTarget()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var source = new Domain.Entities.Patient
        {
            PatientCode = "BN_SOURCE",
            FullName = "Hoàng Văn Nguồn",
            DateOfBirth = new DateTime(1992, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IdentityCardNumber = "001092008888",
            BloodType = BloodType.O_Positive,
            Allergies = "Dị ứng phấn hoa"
        };
        var target = new Domain.Entities.Patient
        {
            PatientCode = "BN_TARGET",
            FullName = "Hoàng Văn Đích",
            DateOfBirth = new DateTime(1992, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PhoneNumber = "0988777666",
            BloodType = BloodType.Unknown // Needs enrichment from source
        };
        dbContext.Patients.AddRange(source, target);
        await dbContext.SaveChangesAsync();

        var handler = new MergePatientsCommandHandler(dbContext, _eventBusMock.Object, _currentUserMock.Object);
        var command = new MergePatientsCommand(source.Id, target.Id, "Trùng lặp hồ sơ do tạo 2 lần tại phòng khám");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedSource = await dbContext.Patients.FindAsync(source.Id);
        updatedSource!.IsMerged.Should().BeTrue();
        updatedSource.MergedToPatientId.Should().Be(target.Id);
        updatedSource.MergeReason.Should().Be("Trùng lặp hồ sơ do tạo 2 lần tại phòng khám");

        var updatedTarget = await dbContext.Patients.FindAsync(target.Id);
        updatedTarget!.BloodType.Should().Be(BloodType.O_Positive); // Enriched from source
        updatedTarget.IdentityCardNumber.Should().Be("001092008888"); // Enriched from source
        updatedTarget.Allergies.Should().Be("Dị ứng phấn hoa"); // Enriched from source

        var history = await dbContext.PatientMergeHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.SourcePatientCode.Should().Be("BN_SOURCE");
        history.TargetPatientCode.Should().Be("BN_TARGET");

        _eventBusMock.Verify(e => e.PublishAsync(
            It.Is<PatientMergedIntegrationEvent>(ev => ev.SourcePatientCode == "BN_SOURCE" && ev.TargetPatientCode == "BN_TARGET"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}

