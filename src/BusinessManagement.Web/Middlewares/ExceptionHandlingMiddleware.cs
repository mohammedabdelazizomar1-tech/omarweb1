using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BusinessManagement.Web.Middlewares;

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
            _logger.LogError(ex, "An unhandled exception occurred during request {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Determine status code based on exception type
        var statusCode = exception switch
        {
            InvalidOperationException or ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        // Check if request was an AJAX call
        bool isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                      context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            var result = JsonSerializer.Serialize(new
            {
                success = false,
                message = exception.Message,
                details = statusCode == HttpStatusCode.InternalServerError ? "Internal Server Error." : exception.Message
            });
            return context.Response.WriteAsync(result);
        }
        else
        {
            // Redirect standard browser views to custom error page
            context.Response.Redirect($"/Home/Error?message={Uri.EscapeDataString(exception.Message)}");
            return Task.CompletedTask;
        }
    }
}





