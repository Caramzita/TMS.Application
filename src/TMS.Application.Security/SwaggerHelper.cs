using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace TMS.Application.Security;

/// <summary>
/// Помощник для настройки Swagger в приложении ASP.NET Core.
/// </summary>
public static class SwaggerHelper
{
    /// <summary>
    /// Добавляет и настраивает Swagger для проекта.
    /// </summary>
    /// <param name="service"> Коллекция сервисов, к которой добавляется Swagger. </param>
    /// <param name="xmlFilePath"> Путь к XML-файлу с комментариями для Swagger. </param>
    /// <returns> Обновленная коллекция сервисов. </returns>
    public static IServiceCollection AddSwagger(this IServiceCollection service, string xmlFilePath)
    {
        service.AddSwaggerGen(opts =>
        {
            opts.IncludeXmlComments(xmlFilePath);

            opts.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = @"Enter access token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            opts.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme,
                        },
                    },
                    new List<string>()
                }
            });
        });

        return service;
    }
}

