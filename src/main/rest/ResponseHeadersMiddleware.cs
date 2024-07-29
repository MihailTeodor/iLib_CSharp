namespace iLib.src.main.rest
{
    public class ResponseHeadersMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, DELETE, PUT");
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
