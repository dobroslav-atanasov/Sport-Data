namespace SportData.Web.Controllers;

using Microsoft.AspNetCore.Mvc;

public abstract class BaseController : Controller
{
    protected BaseController(ILogger<BaseController> logger)
    {
        this.Logger = logger;
    }

    protected ILogger<BaseController> Logger { get; }
}
