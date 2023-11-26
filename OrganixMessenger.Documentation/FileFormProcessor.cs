
using C3.Math;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection;

namespace OrganixMessenger.Documentation
{
    public sealed class FileFormProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;

            var content = operation?.RequestBody?.Content;

            if(content is null)
            {
                return true;
            }

            if (content.ContainsKey("multipart/form-data"))
            {
                var method = context.MethodInfo;

                var parameters = method.GetParameters();
                var parameterFiles = parameters
                    .Where(x => x.ParameterType == typeof(IFormFile) || x.ParameterType == typeof(FormFile));

                var schema = new JsonSchema
                {
                    Type = JsonObjectType.Object
                };

                var propertyFiles = schema.Properties;

                if (!parameterFiles.Any())
                {
                    foreach(var parameter in parameters)
                    {
                        var propertyFilesList = GetFormFilesPropertiesRecursivlyFromType(parameter.ParameterType);

                        foreach (var propertyFile in propertyFilesList)
                        {
                            propertyFiles.Add(propertyFile.Key, propertyFile.Value);
                        }
                    }
                }
                else
                {
                    foreach (var file in parameterFiles)
                    {
                        propertyFiles.Add(file.Name!, new JsonSchemaProperty
                        {
                            Type = JsonObjectType.String,
                            Format = "binary"
                        });
                    }
                }


                content["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                };
            }

            return true;
        }

        private Dictionary<string, JsonSchemaProperty> GetFormFilesPropertiesRecursivlyFromType(Type type)
        {
            if (!type.IsClass)
            {
                return [];
            }

            var props = type.GetProperties();

            var formFileProperties = new Dictionary<string, JsonSchemaProperty>();

            foreach (var prop in props)
            {
                var propSchema = new JsonSchemaProperty();

                
                if (prop.PropertyType == typeof(IFormFile))
                {
                    propSchema.Type = JsonObjectType.String;
                    propSchema.Format = JsonFormatStrings.Binary;
                }
                else if (prop.PropertyType.IsClass)
                {
                    propSchema.Type = JsonObjectType.Object;
                    foreach (var fileProperty in GetFormFilesPropertiesRecursivlyFromType(prop.PropertyType))
                    {
                        propSchema.Properties.Add(fileProperty.Key, fileProperty.Value);
                    }
                }

                formFileProperties.Add(prop.Name, propSchema);
            }

            return formFileProperties;
        }
    }
}
