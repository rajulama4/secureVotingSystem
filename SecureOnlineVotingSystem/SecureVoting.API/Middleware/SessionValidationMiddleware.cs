using SecureVoting.API.Data;
using System.Security.Claims;

namespace SecureVoting.API.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserSessionRepository sessions)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var sessionIdValue = context.User.FindFirstValue("sessionId");

                if (!string.IsNullOrWhiteSpace(userIdValue) &&
                    !string.IsNullOrWhiteSpace(sessionIdValue) &&
                    int.TryParse(userIdValue, out int userId) &&
                    Guid.TryParse(sessionIdValue, out Guid sessionId))
                {
                    bool active = sessions.IsSessionActive(userId, sessionId);

                    if (!active)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Your session expired because this account was logged in from another device."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}