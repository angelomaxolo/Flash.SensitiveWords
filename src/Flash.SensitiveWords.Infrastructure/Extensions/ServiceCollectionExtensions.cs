using Flash.SensitiveWords.Domain.Repositories;
using Flash.SensitiveWords.Infrastructure.Data;
using Flash.SensitiveWords.Infrastructure.Repositories;
using Flash.SensitiveWords.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flash.SensitiveWords.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SensitiveWordsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ISensitiveWordRepository, SensitiveWordRepository>();
        services.AddScoped<IDbInitializer, DbInitializer>();

        return services;
    }
}
