namespace SportData.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class TestController : BaseController
{
    public TestController(ILogger<BaseController> logger)
        : base(logger)
    {
    }

    public IActionResult Add(int id, string name)
    {
        return this.Json(new { id, name });
    }
}
