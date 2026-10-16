using System.Text.Json;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HospitalMS.BuildingBlocks.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, errors) = exception switch
        {
            AppValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList()
            ),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                new List<string> { exception.Message }
            ),
            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                new List<string> { exception.Message }
            ),
            AppException appEx => (
                appEx.StatusCode,
                new List<string> { appEx.Message }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new List<string> { "An unexpected error occurred." }
            )
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        }
        else
        {
            logger.LogWarning(exception, "Application exception ({StatusCode}). TraceId: {TraceId}",
                statusCode, traceId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(errors, traceId);
        await context.Response.WriteAsJsonAsync(response, JsonOptions);
    }
}
