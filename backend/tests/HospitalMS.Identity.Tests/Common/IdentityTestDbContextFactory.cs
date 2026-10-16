using HospitalMS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Tests.Common;

public static class IdentityTestDbContextFactory
{
    public static IdentityDbContext Create()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new IdentityDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}

