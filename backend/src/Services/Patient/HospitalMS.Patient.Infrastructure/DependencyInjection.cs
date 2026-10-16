using HospitalMS.BuildingBlocks.Abstractions;
using HospitalMS.BuildingBlocks.Security;
using HospitalMS.Patient.Application.Common.Interfaces;
using HospitalMS.Patient.Infrastructure.Persistence;
using HospitalMS.Patient.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.Patient.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPatientInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";

        services.AddDbContext<PatientDbContext>(options =>
        {
            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(PatientDbContext).Assembly.FullName));
            }
            else if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(connectionString))
            {
                options.UseInMemoryDatabase("HospitalMS_Patient_Db");
            }
            else
            {
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(PatientDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<IPatientDbContext>(provider =>
            provider.GetRequiredService<PatientDbContext>());

        services.AddScoped<PatientDbInitializer>();
        services.AddScoped<IPatientCodeGenerator, PatientCodeGenerator>();

        // Current User
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // MassTransit (RabbitMQ / InMemory fallback)
        var rabbitHost = configuration["RabbitMQ:Host"];
        services.AddMassTransit(x =>
        {
            if (!string.IsNullOrEmpty(rabbitHost))
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
