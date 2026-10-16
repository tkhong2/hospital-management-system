using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.HealthExam.Application.Common.Interfaces;
using HospitalMS.HealthExam.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.HealthExam.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHealthExamInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";

        services.AddDbContext<HealthExamDbContext>(options =>
        {
            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(HealthExamDbContext).Assembly.FullName));
            }
            else if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(connectionString))
            {
                options.UseInMemoryDatabase("HospitalMS_HealthExam_Db");
            }
            else
            {
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(HealthExamDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<IHealthExamDbContext>(provider =>
            provider.GetRequiredService<HealthExamDbContext>());

        // Current User
        services.AddHttpContextAccessor();

        return services;
    }
}
