using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Проверяем, есть ли в методе параметры для загрузки файлов
        var hasFileParameter = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile) ||
                     p.ParameterType == typeof(IFormFile[]) ||
                     (p.ParameterType.IsClass &&
                      p.ParameterType.GetProperties().Any(prop => prop.PropertyType == typeof(IFormFile))));

        if (hasFileParameter)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Файл для загрузки"
                                },
                                ["chatId"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "ID чата"
                                }
                            },
                            Required = new HashSet<string> { "file", "chatId" }
                        }
                    }
                }
            };
        }
    }
}