using Ai.Api.Application.Interfaces.Repositories;
using Ai.Api.Infrastructure.Persistence.Context;
using Ai.Api.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Ai.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(configuration.GetConnectionString("Default")); });

        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.ConfigureWolverine(options => { options.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>(); });
        return services;
    }
}
