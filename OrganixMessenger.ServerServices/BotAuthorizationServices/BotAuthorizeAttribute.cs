namespace OrganixMessenger.ServerServices.BotAuthorizationServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BotAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter, IFilterFactory
    {
        IBotRepository botRepository;

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            botRepository = serviceProvider.GetRequiredService<IBotRepository>();
            return this;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authHeader = context
                            .HttpContext
                            .Request
                            .Headers
                            .Authorization;

            if (authHeader.Count == 0)
            {
                context.Result = Responses.Unauthorized("Token in 'Authorization' header is not provided. Example 'Bot {token}'.");
                return;
            }

            var authHeaderSplitted = authHeader[0]!.Split(' ');

            if (authHeaderSplitted.Length < 2 || authHeaderSplitted[0] != "Bot")
            {
                context.Result = Responses.Unauthorized("Token in 'Authorization' header is not provided. Example 'Bot {token}'.");
                return;
            }

            var accessToken = authHeaderSplitted[1];

            var bot = await botRepository.FindFirstOrDefaultAsync(x => x.Token == accessToken);

            if (bot is null)
            {
                context.Result = Responses.Unauthorized("Provided token is invalid.");
                return;
            }
            context.HttpContext.User.AddIdentity(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, bot.Id.ToString()),
                        new Claim(ClaimTypes.Name, bot.Name)
                    ]
                ));
        }
    }
}
