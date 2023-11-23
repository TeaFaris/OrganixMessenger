namespace OrganixMessenger.ServerServices.HttpContextServices
{
    public class HttpContextService(IHttpContextAccessor httpContextAccessor) : IHttpContextService
    {
        public string GetBaseUrl()
        {
            return $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host.Value}";
        }
    }
}
