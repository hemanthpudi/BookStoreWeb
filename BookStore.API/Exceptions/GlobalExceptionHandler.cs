using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");

            var statusCode = exception switch
            {
                ArgumentException =>
                    StatusCodes.Status400BadRequest,
                UnauthorizedAccessException =>
                    StatusCodes.Status403Forbidden,

                KeyNotFoundException =>
                    StatusCodes.Status404NotFound,

                InvalidOperationException =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

            var response = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = exception.Message
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }

        private static string GetTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest =>
                    "Bad Request",
                StatusCodes.Status403Forbidden =>
                    "Forbidden",

                StatusCodes.Status404NotFound =>
                    "Resource Not Found",

                StatusCodes.Status409Conflict =>
                    "Conflict",

                _ =>
                    "Internal Server Error"
            };
        }
    }
}