using System.Net;
using System.Text.Json;

namespace UrlShortener.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = "Bir hata oluþtu",
                details = exception.Message,
                timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        message = "Geçersiz iþlem",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        message = "Yetkisiz eriþim",
                        details = "Bu iþlem için yetkiniz bulunmuyor",
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        message = "Geçersiz parametre",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}