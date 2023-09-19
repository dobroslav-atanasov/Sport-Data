namespace SportData.Web.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc.Filters;

public class CustomResourceFilterAttribute : IResourceFilter
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        ;
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        ;
    }
}