using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.OData.Query;
using System.Linq;

namespace CalendarBack;

public class ODataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Проверяем только наличие атрибута EnableQuery
        var isQueryableEndpoint = context.MethodInfo.GetCustomAttributes(true).OfType<EnableQueryAttribute>().Any();
        if (!isQueryableEndpoint) return;

        // Удаляем дублирующиеся параметры из пути
        if (operation.Parameters != null)
        {
            var pathParams = operation.Parameters.Where(p => p.In == ParameterLocation.Path).ToList();
            foreach (var param in pathParams)
            {
                var queryParam = operation.Parameters.FirstOrDefault(p => 
                    p.In == ParameterLocation.Query && p.Name == param.Name);
                if (queryParam != null)
                {
                    operation.Parameters.Remove(queryParam);
                }
            }
        }

        if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

        var odataParameters = new[]
        {
            new OpenApiParameter
            {
                Name = "$filter",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "Фильтрация результатов (например: contains(Title,'Встреча') или ReminderDateTime gt 2024-02-23)",
                Required = false
            },
            new OpenApiParameter
            {
                Name = "$orderby",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "Сортировка результатов (например: ReminderDateTime desc)",
                Required = false
            },
            new OpenApiParameter
            {
                Name = "$skip",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "integer" },
                Description = "Количество пропускаемых записей",
                Required = false
            },
            new OpenApiParameter
            {
                Name = "$top",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "integer" },
                Description = "Количество возвращаемых записей",
                Required = false
            },
            new OpenApiParameter
            {
                Name = "$select",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "Выбор конкретных полей (например: Title,ReminderDateTime)",
                Required = false
            },
            new OpenApiParameter
            {
                Name = "$count",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "boolean" },
                Description = "Включить общее количество записей в ответ",
                Required = false
            }
        };

        foreach (var odataParameter in odataParameters)
        {
            operation.Parameters.Add(odataParameter);
        }
    }
}
