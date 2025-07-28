using Core.Other;
using Core.Ports;
using Core.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class DependenciesConfig
{
    public static void ConfigureCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<GetTokenPayloadQueryHandler>();
        services.AddScoped<GetCurrentAccountQueryHandler>();
        services.AddScoped<ListRolesQueryHandler>();
        services.AddScoped<SessionCreator, SessionCreatorImpl>();
        services.AddScoped<ConfirmationService, ConfirmationServiceImpl>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependenciesConfig).Assembly);
        });
    }
}
