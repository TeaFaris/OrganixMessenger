namespace OrganixMessenger.ServerServices.UIAuthorizationFilters
{
    public sealed class JWTUIAuthorizationFilter : IUiAuthorizationFilter
    {
        public bool Authorize(HttpContext httpContext)
        {
            if(!httpContext.User.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            var role = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

            if (role is null)
            {
                return false;
            }

            return role == Role.Admin.ToString();
        }
    }
}
