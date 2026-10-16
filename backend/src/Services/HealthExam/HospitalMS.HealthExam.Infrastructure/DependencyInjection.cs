using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalMS.HealthExam.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHealthExamInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
