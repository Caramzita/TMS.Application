using MediatR;
using Serilog;

namespace TMS.Application.UseCases.DI;

/// <summary>
/// Поведение для логирования запросов и ответов в конвейере обработки запросов Mediator.
/// </summary>
/// <typeparam name="TRequest">Тип запроса, который обрабатывается.</typeparam>
/// <typeparam name="TResponse">Тип ответа, который возвращается после обработки запроса.</typeparam>
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest
    : IRequest<TResponse>
{
    /// <summary>
    /// Обрабатывает запрос, логируя информацию о запросе, пользователе и передаваемых данных.
    /// </summary>
    /// <param name="request"> Запрос, который нужно обработать. </param>
    /// <param name="next"> Делегат, представляющий следующий шаг в конвейере обработки запросов. </param>
    /// <param name="cancellationToken"> Токен отмены для отмены операции. </param>
    /// <returns> Операция, возвращающая ответ на запрос. </returns>
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        Log.Information("Handling request: {Name} {@Request}", requestName, request);

        var response = await next();

        Log.Information("Request {Name} handled successfully", requestName);

        return response;
    }
}