namespace SportData.Web.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc.Filters;

public class CustomResultFilterAttribute : ResultFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        base.OnResultExecuting(context);
    }

    public override void OnResultExecuted(ResultExecutedContext context)
    {
        base.OnResultExecuted(context);
    }
}