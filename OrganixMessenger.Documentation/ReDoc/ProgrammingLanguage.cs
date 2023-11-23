namespace OrganixMessenger.Documentation.ReDoc
{
    public readonly record struct ProgrammingLanguage : ISmartEnum<int>
    {
        public int ID { get; init; }

        public string Name { get; init; }

        public string DisplayName { get; init; }


        public static readonly ProgrammingLanguage cURL = new()
        {
            ID = 0,
            Name = "Shell",
            DisplayName = "cURL"
        };

        public static readonly ProgrammingLanguage CSharp = new()
        {
            ID = 1,
            Name = "C#",
            DisplayName = "C#"
        };

        public static readonly ProgrammingLanguage Go = new()
        {
            ID = 2,
            Name = nameof(Go),
            DisplayName = nameof(Go)
        };

        public static readonly ProgrammingLanguage Java = new()
        {
            ID = 3,
            Name = nameof(Java),
            DisplayName = nameof(Java)
        };

        public static readonly ProgrammingLanguage JavaScript = new()
        {
            ID = 4,
            Name = nameof(JavaScript),
            DisplayName = "JS"
        };

        public static readonly ProgrammingLanguage PHP = new()
        {
            ID = 5,
            Name = nameof(PHP),
            DisplayName = nameof(PHP)
        };

        public static readonly ProgrammingLanguage Python = new()
        {
            ID = 6,
            Name = nameof(Python),
            DisplayName = nameof(Python)
        };

        public static readonly ProgrammingLanguage Rust = new()
        {
            ID = 7,
            Name = nameof(Rust),
            DisplayName = nameof(Rust)
        };

        public static readonly ProgrammingLanguage TypeScript = new()
        {
            ID = 8,
            Name = nameof(TypeScript),
            DisplayName = nameof(TypeScript)
        };
    }
}
