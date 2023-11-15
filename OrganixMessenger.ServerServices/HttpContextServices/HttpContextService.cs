namespace OrganixMessenger.ServerServices.HttpContextServices
{
    public class HttpContextService : IHttpContextService
    {
        readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetBaseUrl()
        {
            return $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host.Value}";
        }
    }
}
