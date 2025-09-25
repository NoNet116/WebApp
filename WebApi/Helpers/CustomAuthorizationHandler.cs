using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace WebApi.Helpers
{
    public class CustomAuthorizationHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            var user = context.User;

            if (authorizeResult.Forbidden)
            {
                string message;

                if (user.Identity?.IsAuthenticated ?? false)
                {
                    if (!user.IsInRole("Administrator"))
                    {
                        message = "У вас недостаточно прав. Необходимы права администратора.";
                    }
                    else
                    {
                        message = "У вас нет доступа к этому ресурсу.";
                    }
                }
                else
                {
                    message = "Для доступа нужно авторизоваться.";
                }

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message });
                return;
            }

            if (authorizeResult.Challenged)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Для доступа нужно авторизоваться." });
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}