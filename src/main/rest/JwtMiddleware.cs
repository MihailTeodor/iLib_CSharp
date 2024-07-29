using iLib.src.main.IDAO;

namespace iLib.src.main.rest
{
    public class JwtMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context, IUserDao userDao)
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (token != null && JwtHelper.ValidateToken(token))
            {
                AttachUserToContext(context, userDao, token);
            }

            await _next(context);
        }

        private static void AttachUserToContext(HttpContext context, IUserDao userDao, string token)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(token);
                if (userId.HasValue)
                {
                    var user = userDao.FindById(userId.Value);
                    if (user != null)
                    {
                        context.Items["User"] = user;
                    }
                }
            }
            catch
            {
                // Do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }

    }
}
