using HospitalMS.Pharmacy.Application.Common.Interfaces;
using HospitalMS.Pharmacy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.Pharmacy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPharmacyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";

        services.AddDbContext<PharmacyDbContext>(options =>
        {
            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(PharmacyDbContext).Assembly.FullName));
            }
            else if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(connectionString))
            {
                options.UseInMemoryDatabase("HospitalMS_Pharmacy_Db");
            }
            else
            {
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(PharmacyDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<IPharmacyDbContext>(provider =>
            provider.GetRequiredService<PharmacyDbContext>());

        // Current User
        services.AddHttpContextAccessor();

        return services;
    }
}
