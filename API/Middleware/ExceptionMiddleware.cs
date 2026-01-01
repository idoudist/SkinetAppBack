using System.Text.Json;

namespace API.Middleware;

public class ExceptionMiddleware(IHostEnvironment env, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex, IHostEnvironment env)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = env.IsDevelopment()
            ? new Errors.ApiErrorResponse(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
            : new Errors.ApiErrorResponse(context.Response.StatusCode, ex.Message, "Internal Server Error");

        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}
