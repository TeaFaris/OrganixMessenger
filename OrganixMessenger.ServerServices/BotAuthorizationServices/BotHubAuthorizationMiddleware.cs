namespace OrganixMessenger.ServerServices.BotAuthorizationServices
{
    public class BotHubAuthorizationMiddleware(IBotRepository botRepository) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var authHeader = context
                            .Request
                            .Headers
                            .Authorization;

            if (authHeader.Count == 0)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(Responses.Unauthorized("Token in 'Authorization' header is not provided. Example 'Bot {token}'."));
                return;
            }

            var authHeaderSplitted = authHeader[0]!.Split(' ');

            if (authHeaderSplitted.Length < 2 || authHeaderSplitted[0] != "Bot")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(Responses.Unauthorized("Token in 'Authorization' header is not provided. Example 'Bot {token}'."));
                return;
            }

            var accessToken = authHeaderSplitted[1];

            var bot = await botRepository.FindFirstOrDefaultAsync(x => x.Token == accessToken);

            if (bot is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(Responses.Unauthorized("Provided token is invalid."));
                return;
            }

            context.User.AddIdentity(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, bot.Id.ToString()),
                        new Claim(ClaimTypes.Name, bot.Name)
                    ]
                ));

            await next(context);
        }
    }
}
