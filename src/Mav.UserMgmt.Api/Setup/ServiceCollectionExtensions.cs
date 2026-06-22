using Mav.UserMgmt.Api.Interfaces;
using Mav.UserMgmt.Api.Services;

namespace Mav.UserMgmt.Api.Setup;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();

        services.AddScoped<IHealthCheckService, HealthCheckService>();
    }
}
