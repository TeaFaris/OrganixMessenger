namespace OrganixMessenger.Documentation.ReDoc
{
    public class ReDocCodeSample(ProgrammingLanguage language, string source)
    {
        [JsonProperty("label")]
        public string Label { get; set; } = language.DisplayName;

        [JsonProperty("lang")]
        public string Language { get; set; } = language.Name;

        [JsonProperty("source")]
        public string Source { get; set; } = source;
    }
}
