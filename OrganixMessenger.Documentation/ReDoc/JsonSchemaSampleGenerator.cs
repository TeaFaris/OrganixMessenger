namespace OrganixMessenger.Documentation.ReDoc
{
    public static class JsonSchemaSampleGenerator
    {
        public static JToken? ToJsonSample(this JsonSchema schema, Type payloadType, Type? arrayType = null)
        {
            JToken? output;
            switch (schema.Type)
            {
                case JsonObjectType.Object:
                case JsonObjectType.Object | JsonObjectType.Null:
                    {
                        var jObject = new JObject();
                        if (schema.Properties is not null)
                        {
                            foreach (var prop in schema.Properties)
                            {
                                string jsonPropName = TranslateNameToJson(prop.Key);

                                var propertyType = payloadType
                                                    .GetProperty(prop.Key)!
                                                    .PropertyType;

                                var propertySchema = JsonSchema.FromType(propertyType);

                                var ienumerableInterface = propertyType.GetInterfaces().FirstOrDefault(x => x.IsGenericType
                                            && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                                var ienumerableType = ienumerableInterface?.GenericTypeArguments[0];

                                var jsonSample = prop.Value.Type != JsonObjectType.None
                                                           ? ToJsonSample(prop.Value, propertyType, ienumerableType)
                                                           : ToJsonSample(propertySchema, propertyType, ienumerableType);

                                jObject.Add(
                                        jsonPropName,
                                        jsonSample
                                    );
                            }
                        }
                        output = jObject;
                    }
                    break;
                case JsonObjectType.Array:
                case JsonObjectType.Array | JsonObjectType.Null:
                    {
                        var jArray = new JArray();

                        var item = JsonSchema.FromType(arrayType);

                        var ienumerableInterface = arrayType!.GetInterfaces().FirstOrDefault(x => x.IsGenericType
                                            && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                        jArray.Add(ToJsonSample(item, arrayType, ienumerableInterface?.GenericTypeArguments[0])!);

                        output = jArray;
                    }
                    break;
                case JsonObjectType.String:
                case JsonObjectType.String | JsonObjectType.Null:
                    {
                        output = new JValue(payloadType switch
                        {
                            var t when t == typeof(Guid) => Guid.NewGuid(),
                            var t when t == typeof(DateTime) => DateTime.Now,
                            _ => "string"
                        });
                    }
                    break;
                case JsonObjectType.Number:
                case JsonObjectType.Number | JsonObjectType.Null:
                    output = new JValue(1.0);
                    break;
                case JsonObjectType.Integer:
                case JsonObjectType.Integer | JsonObjectType.Null:
                    output = new JValue(1);
                    break;
                case JsonObjectType.Boolean:
                case JsonObjectType.Boolean | JsonObjectType.Null:
                    output = new JValue(false);
                    break;
                case JsonObjectType.Null:
                    output = JValue.CreateNull();
                    break;
                default:
                    output = null;
                    break;
            }

            return output;
        }

        private static string TranslateNameToJson(string name)
        {
            return string.Concat(name[..1].ToLower(), name.AsSpan(1));
        }
    }
}
