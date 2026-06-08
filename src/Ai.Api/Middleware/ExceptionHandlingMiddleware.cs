using System.Net;
using System.Text.Json;
using Ai.Api.Domain.Exceptions;
using FluentValidation;

namespace Ai.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

            (HttpStatusCode statusCode, string detail) = MapException(ex);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = statusCode switch
                {
                    HttpStatusCode.BadRequest => "Bad Request",
                    HttpStatusCode.NotFound => "Not Found",
                    HttpStatusCode.Conflict => "Conflict",
                    _ => "Internal Server Error"
                },
                Detail = detail,
                Instance = context.Request.Path
            };

            string json = JsonSerializer.Serialize(problem);
            await context.Response.WriteAsync(json);
        }
    }

    private static (HttpStatusCode StatusCode, string Detail) MapException(Exception ex)
    {
        return ex switch
        {
            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, validationEx.Message),

            DomainException domainEx =>
                (HttpStatusCode.BadRequest, domainEx.Message),

            InvalidOperationException { Message: var msg } when msg.Contains("was not found") =>
                (HttpStatusCode.NotFound, msg),

            InvalidOperationException { Message: var msg } when msg.Contains("already exists") =>
                (HttpStatusCode.Conflict, msg),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
    }
}
