using FluentAssertions;
using HospitalMS.BuildingBlocks.Common;
using Xunit;

namespace HospitalMS.BuildingBlocks.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        const string errorMessage = "Hospital service encountered an error";
        var result = Result.Failure(errorMessage);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        const string patientId = "BN-2026-0001";
        var result = Result.Success(patientId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(patientId);
    }

    [Fact]
    public void PagedResult_ShouldCalculateTotalPagesCorrectly()
    {
        var items = new List<string> { "Exam1", "Exam2", "Exam3" };
        var pagedResult = new PagedResult<string>(items, totalCount: 25, pageNumber: 1, pageSize: 10);

        pagedResult.TotalPages.Should().Be(3);
        pagedResult.HasPreviousPage.Should().BeFalse();
        pagedResult.HasNextPage.Should().BeTrue();
    }
}

