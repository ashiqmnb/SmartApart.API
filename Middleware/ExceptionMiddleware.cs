using SmartApart.API.Common;

namespace SmartApart.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                await HandleAsync(context, ex.Message, ex.StatusCode);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleAsync(context, ex.Message, 401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await HandleAsync(context, "An unexpected error occurred.", 500);
            }
        }

        private static async Task HandleAsync(HttpContext ctx, string message, int statusCode)
        {
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Fail(message, statusCode);
            await ctx.Response.WriteAsJsonAsync(response);
        }
    }
}
