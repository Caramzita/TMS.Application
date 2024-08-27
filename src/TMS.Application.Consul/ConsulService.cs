using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TMS.Application.Consul;

public static class ConsulService
{
    /// <summary>
    /// Добавляет услуги Consul в контейнер зависимостей.
    /// </summary>
    /// <param name="services"> Коллекция сервисов для добавления услуг. </param>
    /// <param name="configuration"> Объект конфигурации для получения настроек Consul. </param>
    /// <returns> Обновленная коллекция сервисов. </returns>
    public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
    {
        // Получение настроек Consul из конфигурации
        var consulConfig = configuration.GetSection("ConsulConfig").Get<ConsulConfig>()
            ?? throw new InvalidOperationException("ConsulConfig section is not configured.");

        // Регистрация клиента Consul
        services.AddSingleton<IConsulClient>(provider => new ConsulClient(config =>
        {
            config.Address = new Uri(consulConfig.Address);
        }));

        // Регистрация конфигурации Consul
        services.AddSingleton(p => Options.Create(consulConfig));

        // Регистрация хост-сервиса ConsulHostedService
        services.AddSingleton<IHostedService, ConsulHostedService>(provider =>
        {
            var consulClient = provider.GetRequiredService<IConsulClient>();
            var consulConfigOptions = provider.GetRequiredService<IOptions<ConsulConfig>>();
            var logger = provider.GetRequiredService<ILogger<ConsulHostedService>>();
            return new ConsulHostedService(consulClient, consulConfigOptions, logger);
        });

        return services;
    }
}
