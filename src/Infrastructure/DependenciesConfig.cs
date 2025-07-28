using Core.Ports;
using Infrastructure.OAuth;
using Infrastructure.Options;
using Infrastructure.Other;
using Infrastructure.Persistence.Dapper;
using Infrastructure.Persistence.EF;
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
        services.AddScoped<ConfirmationCodeEmailSender, ConfirmationCodeEmailSenderImpl>();
        services.AddScoped<EmailSender, SystemEmailSender>();
        services.AddScoped<OAuthServiceFactory, OAuthServiceFactoryImpl>();
        services.AddScoped<ConfirmationCodesRepository, EfConfirmationCodesRepository>();
        services.AddScoped<ConfirmationCodeEmailSender, ConfirmationCodeEmailSenderImpl>();

        services.AddSingleton<OAuthStateTokenService, SystemOAuthStateTokenService>();
        services.AddSingleton<AuthTokenService, SystemAuthTokenService>();
        services.AddSingleton<DateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<SqlConnectionFactory, DapperSqlConnectionFactory>();

        services.AddHttpClient();
    }
}
