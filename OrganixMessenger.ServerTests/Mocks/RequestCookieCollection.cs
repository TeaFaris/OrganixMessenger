using Microsoft.AspNetCore.Http;

namespace OrganixMessenger.ServerTests.Mocks
{
    public sealed class RequestCookieCollection : Dictionary<string, string>, IRequestCookieCollection
    {
        ICollection<string> IRequestCookieCollection.Keys => Keys;
    }
}
