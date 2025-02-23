using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CalendarBack;

public class ODataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
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
