namespace SportData.Web.Infrastructure.Middlewares;

using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SportData.Data.Models.Errors;

public class ExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception ex)
        {

            this.logger.LogError($"{ex}");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error"
        }.ToString());
    }
}