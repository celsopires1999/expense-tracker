using Auth.Api.Extensions;
using Auth.Api.Infrastructure;

namespace Auth.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSwaggerGenWithAuth();

        return services;
    }
}
