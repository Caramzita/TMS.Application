using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace TMS.Application.UseCases.DI;

/// <summary>
/// Обрабатывает исключения, возникающие во время обработки HTTP-запросов.
/// </summary>
public class ErrorExceptionHandler
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Конструктор для инициализации обработчика исключений.
    /// </summary>
    /// <param name="next"> Делегат, представляющий следующий компонент в конвейере обработки запросов. </param>
    /// <exception cref="ArgumentNullException"> Выбрасывается, если <paramref name="next"/> равен null. </exception>
    public ErrorExceptionHandler(RequestDelegate next) =>
        _next = next ?? throw new ArgumentNullException(nameof(next));

    /// <summary>
    /// Метод для обработки HTTP-запроса и перехвата исключений.
    /// </summary>
    /// <param name="context"> Контекст HTTP-запроса. </param>
    /// <returns> Задача, представляющая асинхронную операцию. </returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Обрабатывает исключения и возвращает соответствующий ответ клиенту.
    /// </summary>
    /// <param name="context"> Контекст HTTP-запроса. </param>
    /// <param name="exception"> Произошедшее исключение. </param>
    /// <returns> Задача, представляющая асинхронную операцию записи ответа. </returns>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                var errors = validationException.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                });
                result = JsonSerializer.Serialize(new { errors });
                break;
            default:
                result = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(result);
    }
}
