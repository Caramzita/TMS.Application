using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;

namespace TMS.Application.Security;

/// <summary>
/// Класс с методами расширения для настройки аутентификации JWT и загрузки настроек из Consul.
/// </summary>
public static class AuthenticationHelper
{
    /// <summary>
    /// Добавляет аутентификацию JWT Bearer в сервисы.
    /// </summary>
    /// <param name="services"> Коллекция сервисов для настройки. </param>
    /// <param name="jwtSettings"> Настройки JWT, такие как секретный ключ, издатель, и аудитория. </param>
    /// <returns> Обновленная коллекция сервисов. </returns>
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services)
    {
        var jwtSettings = services.BuildServiceProvider().GetRequiredService<IOptions<JwtTokenSettings>>().Value;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
        });

        return services;
    }

    /// <summary>
    /// Добавляет настройки JWT из Consul в сервисы.
    /// </summary>
    /// <param name="services"> Коллекция сервисов для настройки. </param>
    /// <param name="consulKey"> Ключ в Consul, по которому хранятся настройки JWT. </param>
    /// <returns> Обновленная коллекция сервисов. </returns>
    public static IServiceCollection AddJwtSettingsFromConsul(this IServiceCollection services, string consulKey)
    {
        services.AddSingleton<IConfigureOptions<JwtTokenSettings>>(sp =>
        {
            var consulClient = sp.GetRequiredService<IConsulClient>();
            var config = consulClient.KV.Get(consulKey).GetAwaiter().GetResult();

            if (config.Response == null)
            {
                throw new Exception($"Consul key '{consulKey}' not found");
            }

            var jwtSettings = JsonConvert.DeserializeObject<JwtTokenSettings>(Encoding.UTF8.GetString(config.Response.Value));
            return new ConfigureOptions<JwtTokenSettings>(options =>
            {
                options.SecretKey = jwtSettings.SecretKey;
                options.Issuer = jwtSettings.Issuer;
                options.Audience = jwtSettings.Audience;
                options.AccessTokenLifetimeInMinutes = jwtSettings.AccessTokenLifetimeInMinutes;
                options.RefreshTokenLifetimeInMinutes = jwtSettings.RefreshTokenLifetimeInMinutes;
            });
        });

        return services;
    }
}