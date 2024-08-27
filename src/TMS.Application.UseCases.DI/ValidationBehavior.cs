using FluentValidation;
using MediatR;

namespace TMS.Application.UseCases.DI;

/// <summary>
/// Поведение для валидации запросов в конвейере обработки запросов Mediator.
/// </summary>
/// <typeparam name="TRequest"> Тип запроса, который обрабатывается. </typeparam>
/// <typeparam name="TResponse"> Тип ответа, который возвращается после обработки запроса. </typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Коллекция валидаторов, используемых для проверки запроса.
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ValidationBehavior{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="validators"> Коллекция валидаторов, используемых для проверки запроса. </param>
    /// <exception cref="ArgumentNullException"> Выбрасывается, 
    /// если <paramref name="validators"/> равен <c>null</c>. </exception>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) =>
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));

    /// <summary>
    /// Выполняет валидацию запроса перед передачей его следующему шагу в конвейере.
    /// </summary>
    /// <param name="request"> Запрос, который нужно валидировать и обработать. </param>
    /// <param name="next"> Делегат, представляющий следующий шаг в конвейере обработки запросов. </param>
    /// <param name="cancellationToken"> Токен отмены для отмены операции. </param>
    /// <returns> Операция, возвращающая ответ на запрос. </returns>
    /// <exception cref="ValidationException"> Выбрасывается, если в запросе обнаружены ошибки валидации. </exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        if (_validators?.Any() ?? false)
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}