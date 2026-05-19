using Flash.SensitiveWords.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Flash.SensitiveWords.Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services.
/// Should be called from the API/Presentation layer, not Infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Application layer services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISensitiveWordService, SensitiveWordService>();
        return services;
    }
}
