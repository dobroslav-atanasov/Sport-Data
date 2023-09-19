namespace SportData.Web.Infrastructure.Filters;

using Microsoft.AspNetCore.Mvc.Filters;

public class CustomAuthorizationFilterAttribute : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        ;
    }
}