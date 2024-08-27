namespace TMS.Application.Consul;

/// <summary>
/// Конфигурация для подключения к Consul.
/// </summary>
public class ConsulConfig
{
    /// <summary>
    /// Адрес сервера Consul.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Имя сервиса, зарегистрированного в Consul.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Адрес, по которому доступен сервис (обычно это адрес хоста или IP-адрес).
    /// </summary>
    public string ServiceAddress { get; set; } = string.Empty;

    /// <summary>
    /// Порт, на котором работает сервис.
    /// </summary>
    public int ServicePort { get; set; }

    /// <summary>
    /// Массив тегов для сервиса, которые могут использоваться для фильтрации или дополнительной информации.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();
}

