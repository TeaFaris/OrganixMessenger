namespace OrganixMessenger.ServerServices.APITokenGeneratorServices
{
    public sealed class APITokenGeneratorService : IAPITokenGeneratorService
    {
        private const string Prefix = "CT-";
        private const int NumberOfSecureBytesToGenerate = 32;
        private const int LengthOfKey = 32;

        public string GenerateAPIToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(NumberOfSecureBytesToGenerate);

            string base64String = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_");

            var keyLength = LengthOfKey - Prefix.Length;

            return Prefix + base64String[..keyLength];
        }
    }
}
