using HospitalMS.Inpatient.Application.Common.Interfaces;
using HospitalMS.Inpatient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.Inpatient.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInpatientInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSql";
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<InpatientDbContext>(options =>
        {
            if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("HospitalMS_Inpatient_Db");
            }
            else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IInpatientDbContext>(sp => sp.GetRequiredService<InpatientDbContext>());

        return services;
    }
}
