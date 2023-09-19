namespace SportData.Web.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc.Filters;

public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        base.OnException(context);
    }
}