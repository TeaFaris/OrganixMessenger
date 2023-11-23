namespace OrganixMessenger.Documentation.ReDoc
{
    public class ReDocCodeSampleAppender : IOperationProcessor
    {
        private const string ExtensionKey = "x-codeSamples";
        private const string BaseURL = "https://organizatsiya.org";

        public bool Process(OperationProcessorContext context)
        {
            var aspNetProcessorContext = (AspNetCoreOperationProcessorContext)context;

            var parameters = aspNetProcessorContext.ApiDescription.ParameterDescriptions;
            var parametersFromQuery = parameters.Where(x => x.Source == BindingSource.Query);

            var requestURL = new Uri(              
                    new Uri(BaseURL),
                    QueryHelpers.AddQueryString(
                        context.OperationDescription.Path,
                        parametersFromQuery.Select(x => new KeyValuePair<string, StringValues>(
                                x.Name,
                                JsonSchema.FromType(x.Type)
                                          .ToJsonSample(x.Type, null)!
                                          .ToString()
                        ))
                    )
                );

            context.OperationDescription.Operation.ExtensionData ??= new Dictionary<string, object?>();

            var payloadDescriptor = parameters.FirstOrDefault(x => x.Source == BindingSource.Body);

            JsonSchema? payloadSchema = payloadDescriptor is not null
                ? JsonSchema.FromType(payloadDescriptor.Type)
                : null;

            var enumerableInterface = Array.Find(payloadDescriptor?.Type.GetInterfaces() ?? [],
                x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            var arrayType = enumerableInterface?.GenericTypeArguments[0];

            var data = context.OperationDescription.Operation.ExtensionData;
            if (!data.TryGetValue(ExtensionKey, out object? value))
            {
                value = new List<ReDocCodeSample>();
                data[ExtensionKey] = value;
            }

            var samples = (List<ReDocCodeSample>)value!;

            var languages = SmartEnumExtentions.GetValues<ProgrammingLanguage, int>();

            var payloadJson = payloadSchema?.ToJsonSample(payloadDescriptor!.Type, arrayType);
            foreach (var language in languages)
            {
                samples.Add(new ReDocCodeSample(
                        language,
                        RequestCodeGenerator.GetCodeSample(
                                language,
                                requestURL,
                                context.OperationDescription,
                                payloadJson
                            )
                    ));
            }

            return true;
        }
    }
}