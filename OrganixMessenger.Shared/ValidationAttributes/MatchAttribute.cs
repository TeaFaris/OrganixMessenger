namespace OrganixMessenger.Shared.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public sealed class MatchAttribute(params object[] matchTo) : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return Array.Exists(matchTo, x => x.Equals(value));
        }
    }
}
