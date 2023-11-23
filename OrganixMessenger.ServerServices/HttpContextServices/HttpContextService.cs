namespace OrganixMessenger.ServerServices.HttpContextServices
{
    public class HttpContextService(IHttpContextAccessor httpContextAccessor) : IHttpContextService
    {
        readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

        public string GetBaseUrl()
        {
            return $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host.Value}";
        }
    }
}
