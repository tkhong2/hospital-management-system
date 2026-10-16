using FluentAssertions;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Application.DTOs;
using HospitalMS.Catalog.Application.Features.Import.Commands;
using HospitalMS.Catalog.Application.Features.MedicalServices.Queries.GetMedicalServices;
using HospitalMS.Catalog.Domain.Entities;
using HospitalMS.Catalog.Domain.Enums;
using HospitalMS.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HospitalMS.Catalog.Tests;

public class CatalogTests
{
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetMedicalServices_ReturnsFilteredServicesAndCachesResult()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        dbContext.MedicalServices.AddRange(
            new MedicalService { Code = "KB01", Name = "Khám Nội", ServiceType = ServiceType.Examination, Price = 100000 },
            new MedicalService { Code = "XN01", Name = "Xét nghiệm máu", ServiceType = ServiceType.Laboratory, Price = 50000 },
            new MedicalService { Code = "HA01", Name = "Chụp X-quang", ServiceType = ServiceType.Imaging, Price = 120000 }
        );
        await dbContext.SaveChangesAsync();

        var handler = new GetMedicalServicesQueryHandler(dbContext, _cacheServiceMock.Object);
        var query = new GetMedicalServicesQuery(PageNumber: 1, PageSize: 10, ServiceType: ServiceType.Laboratory);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().ContainSingle();
        result.Value.Items[0].Code.Should().Be("XN01");

        _cacheServiceMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<PagedResult<MedicalServiceDto>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ImportMedicalServices_InsertsNewAndUpdatesExisting()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        dbContext.MedicalServices.Add(new MedicalService
        {
            Code = "KB01",
            Name = "Khám Cũ",
            Price = 50000,
            ServiceType = ServiceType.Examination
        });
        await dbContext.SaveChangesAsync();

        var handler = new ImportCatalogCommandHandler(dbContext, _cacheServiceMock.Object);
        var command = new ImportMedicalServicesCommand(
            Items: new List<ImportMedicalServiceItem>
            {
                new("KB01", "Khám Cập Nhật", ServiceType.Examination, "Lần", 70000, 39000, "KKB", "Đã cập nhật giá"),
                new("XN99", "Xét nghiệm mới", ServiceType.Laboratory, "XN", 150000, 80000, "KXN", "Mới bổ sung")
            }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalProcessed.Should().Be(2);
        result.Value.UpdatedCount.Should().Be(1);
        result.Value.InsertedCount.Should().Be(1);

        var kb01 = await dbContext.MedicalServices.FirstAsync(s => s.Code == "KB01");
        kb01.Name.Should().Be("Khám Cập Nhật");
        kb01.Price.Should().Be(70000);

        var xn99 = await dbContext.MedicalServices.FirstAsync(s => s.Code == "XN99");
        xn99.Name.Should().Be("Xét nghiệm mới");

        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("catalog:services", It.IsAny<CancellationToken>()), Times.Once);
    }
}
