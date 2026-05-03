using System.Net;
using System.Text.Json;
using ConfigService.Domain.Exceptions;

namespace ConfigService.Api.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFoundEx => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                message = notFoundEx.Message,
                errors = (IDictionary<string, string[]>?)null
            },
            ValidationException validationEx => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = validationEx.Message,
                errors = (IDictionary<string, string[]>?)validationEx.Errors
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "An error occurred while processing your request.",
                errors = (IDictionary<string, string[]>?)null
            }
        };

        context.Response.StatusCode = response.statusCode;

        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
