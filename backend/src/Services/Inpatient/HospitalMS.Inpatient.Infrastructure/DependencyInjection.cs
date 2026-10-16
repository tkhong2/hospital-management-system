using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.Inpatient.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInpatientInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
