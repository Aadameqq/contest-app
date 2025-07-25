using Core.Ports;
using Infrastructure.OAuth;
using Infrastructure.Options;
using Infrastructure.Other;
using Infrastructure.Persistence.Dapper;
using Infrastructure.Persistence.EF;
using Infrastructure.Persistence.Redis;
using Infrastructure.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependenciesConfig
{
    public static void ConfigureInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        });

        services.AddDbContext<DatabaseContext>();

        services.AddTransient<UnitOfWork, EfUnitOfWork>();

        services.AddScoped<PasswordHasher, BCryptPasswordService>();
        services.AddScoped<PasswordVerifier, BCryptPasswordService>();
        services.AddScoped<ActivationCodeEmailSender, ActivationCodeEmailSenderImpl>();
        services.AddScoped<EmailSender, SystemEmailSender>();
        services.AddScoped<ActivationCodesRepository, RedisActivationCodesRepository>();
        services.AddScoped<PasswordResetCodesRepository, RedisPasswordResetCodesRepository>();
        services.AddScoped<PasswordResetEmailSender, PasswordResetEmailSenderImpl>();
        services.AddScoped<OAuthServiceFactory, OAuthServiceFactoryImpl>();

        services.AddSingleton<OAuthStateTokenService, SystemOAuthStateTokenService>();
        services.AddSingleton<AuthTokenService, SystemAuthTokenService>();
        services.AddSingleton<DateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<SqlConnectionFactory, DapperSqlConnectionFactory>();

        services.AddHttpClient();
    }
}
