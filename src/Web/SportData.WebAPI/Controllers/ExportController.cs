namespace SportData.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ILogger<ExportController> _logger;

    public ExportController(ILogger<ExportController> logger)
    {
        _logger = logger;
    }

    [HttpGet("Add")]
    public IActionResult Add()
    {
        return this.Ok("Add");
    }
}
