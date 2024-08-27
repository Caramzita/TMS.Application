namespace TMS.Application.UseCases;

/// <summary>
/// Расширение перечисления <see cref="ResultStatus"/>
/// </summary>
public static class ResultStatusExctention
{
    /// <summary>
    /// Успешно
    /// </summary>
    /// <param name="status">Статус</param>
    /// <returns><see langword="true"/> если успешно, иначе <see langword="false"/></returns>
    public static bool IsSuccess(this ResultStatus status)
    {
        return (int)status is >= 200 and < 300; 
    }
}
