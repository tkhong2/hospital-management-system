using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.Outpatient.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOutpatientInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
