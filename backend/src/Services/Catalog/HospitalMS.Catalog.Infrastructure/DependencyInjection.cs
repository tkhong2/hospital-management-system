using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Security;
using HospitalMS.Catalog.Application.Common.Interfaces;
using HospitalMS.Catalog.Infrastructure.Persistence;
using HospitalMS.Catalog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";

        services.AddDbContext<CatalogDbContext>(options =>
        {
            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName));
            }
            else if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(connectionString))
            {
                options.UseInMemoryDatabase("HospitalMS_Catalog_Db");
            }
            else
            {
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<ICatalogDbContext>(provider =>
            provider.GetRequiredService<CatalogDbContext>());

        services.AddScoped<CatalogDbInitializer>();

        // Current User
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // Caching: MemoryCache + Distributed Cache (Redis)
        services.AddMemoryCache();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "HospitalMS_Catalog_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
