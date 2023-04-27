namespace SportData.Web.Infrastructure.Middlewares;

using Microsoft.AspNetCore.Http;

public class CustomeMiddleware
{
    private readonly RequestDelegate next;

    public CustomeMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await context.Response.WriteAsync("1");
        if (DateTime.Now.Second % 2 == 0)
        {
            await this.next(context);
        }

        await context.Response.WriteAsync("2");
    }
}