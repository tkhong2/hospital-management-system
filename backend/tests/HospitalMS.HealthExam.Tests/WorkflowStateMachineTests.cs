using FluentAssertions;
using HospitalMS.HealthExam.Domain.Entities;
using HospitalMS.HealthExam.Domain.Enums;
using HospitalMS.HealthExam.Domain.StateMachine;
using HospitalMS.HealthExam.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HospitalMS.HealthExam.Tests;

public class WorkflowStateMachineTests
{
    [Theory]
    [InlineData(RegistrationStatus.Pending, RegistrationStatus.CheckedIn, true)]
    [InlineData(RegistrationStatus.Pending, RegistrationStatus.Cancelled, true)]
    [InlineData(RegistrationStatus.Pending, RegistrationStatus.Completed, false)]
    [InlineData(RegistrationStatus.CheckedIn, RegistrationStatus.InProgress, true)]
    [InlineData(RegistrationStatus.InProgress, RegistrationStatus.WaitingForLab, true)]
    [InlineData(RegistrationStatus.InProgress, RegistrationStatus.WaitingForConclusion, true)]
    [InlineData(RegistrationStatus.WaitingForConclusion, RegistrationStatus.Completed, true)]
    [InlineData(RegistrationStatus.Completed, RegistrationStatus.Delivered, true)]
    [InlineData(RegistrationStatus.Delivered, RegistrationStatus.Pending, false)]
    public void CanTransition_ValidatesAllowedTransitionsCorrectly(
        RegistrationStatus current,
        RegistrationStatus target,
        bool expected)
    {
        var result = ExamWorkflowStateMachine.CanTransition(current, target);
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidateTransition_ThrowsInvalidOperationException_WhenInvalid()
    {
        var act = () => ExamWorkflowStateMachine.ValidateTransition(RegistrationStatus.Pending, RegistrationStatus.Completed);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Chuyển đổi trạng thái không hợp lệ*");
    }

    [Fact]
    public void CalculateNextStatus_WhenPendingAndNotCheckedIn_ReturnsPending()
    {
        var status = ExamWorkflowStateMachine.CalculateNextStatus(
            hasCheckedIn: false,
            allClinicalStationsCompleted: false,
            hasPendingLabOrders: false,
            hasDoctorConclusion: false);

        status.Should().Be(RegistrationStatus.Pending);
    }

    [Fact]
    public void CalculateNextStatus_WhenClinicalIncomplete_ReturnsInProgress()
    {
        var status = ExamWorkflowStateMachine.CalculateNextStatus(
            hasCheckedIn: true,
            allClinicalStationsCompleted: false,
            hasPendingLabOrders: false,
            hasDoctorConclusion: false);

        status.Should().Be(RegistrationStatus.InProgress);
    }

    [Fact]
    public void CalculateNextStatus_WhenClinicalDoneButLabPending_ReturnsWaitingForLab()
    {
        var status = ExamWorkflowStateMachine.CalculateNextStatus(
            hasCheckedIn: true,
            allClinicalStationsCompleted: true,
            hasPendingLabOrders: true,
            hasDoctorConclusion: false);

        status.Should().Be(RegistrationStatus.WaitingForLab);
    }

    [Fact]
    public void CalculateNextStatus_WhenClinicalDoneAndLabDone_ReturnsWaitingForConclusion()
    {
        var status = ExamWorkflowStateMachine.CalculateNextStatus(
            hasCheckedIn: true,
            allClinicalStationsCompleted: true,
            hasPendingLabOrders: false,
            hasDoctorConclusion: false);

        status.Should().Be(RegistrationStatus.WaitingForConclusion);
    }

    [Fact]
    public void CalculateNextStatus_WhenDoctorConcluded_ReturnsCompleted()
    {
        var status = ExamWorkflowStateMachine.CalculateNextStatus(
            hasCheckedIn: true,
            allClinicalStationsCompleted: true,
            hasPendingLabOrders: false,
            hasDoctorConclusion: true);

        status.Should().Be(RegistrationStatus.Completed);
    }

    [Fact]
    public async Task DbInitializer_SeedsDataProperly()
    {
        var options = new DbContextOptionsBuilder<HealthExamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new HealthExamDbContext(options);
        await HealthExamDbInitializer.SeedAsync(context);

        var companies = await context.Companies.ToListAsync();
        companies.Should().HaveCount(2);

        var stations = await context.ExamStations.ToListAsync();
        stations.Should().HaveCount(7);

        var contracts = await context.ExamContracts.ToListAsync();
        contracts.Should().HaveCount(2);

        var packages = await context.ExamPackages.ToListAsync();
        packages.Should().HaveCount(3);

        var registrations = await context.ExamRegistrations.ToListAsync();
        registrations.Should().HaveCount(3);
    }
}

