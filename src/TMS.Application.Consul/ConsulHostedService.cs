using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TMS.Application.Consul;

/// <summary>
/// Сервис, регистрирующий и отменяющий регистрацию сервиса в Consul при запуске и остановке приложения.
/// </summary>
public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;

    private readonly ILogger<ConsulHostedService> _logger;

    private readonly ConsulConfig _consulConfig;

    private string? _registrationId;

    /// <summary>
    /// Конструктор для инициализации сервиса ConsulHostedService.
    /// </summary>
    /// <param name="consulClient">Клиент для взаимодействия с Consul.</param>
    /// <param name="consulConfig">Конфигурация для подключения к Consul.</param>
    /// <param name="logger">Логгер для записи информации и ошибок.</param>
    public ConsulHostedService(IConsulClient consulClient, 
        IOptions<ConsulConfig> consulConfig, 
        ILogger<ConsulHostedService> logger)
    {
        _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _consulConfig = consulConfig.Value ?? throw new ArgumentNullException(nameof(consulConfig));
    }

    /// <summary>
    /// Метод вызывается при запуске приложения. Регистрирует сервис в Consul.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для асинхронных операций.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _registrationId = $"{_consulConfig.ServiceName}-{Guid.NewGuid()}";

        var registration = new AgentServiceRegistration
        {
            ID = _registrationId,
            Name = _consulConfig.ServiceName,
            Address = _consulConfig.ServiceAddress,
            Port = _consulConfig.ServicePort,
            Tags = _consulConfig.Tags
        };

        try
        {
            _logger.LogInformation("Registering with Consul");
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering with Consul");
        }
    }

    /// <summary>
    /// Метод вызывается при остановке приложения. Отменяет регистрацию сервиса в Consul.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для асинхронных операций.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_registrationId))
            return;

        try
        {
            _logger.LogInformation("Deregistering from Consul");
            await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deregistering from Consul");
        }
    }
}
