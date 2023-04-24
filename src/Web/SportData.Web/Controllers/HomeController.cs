namespace SportData.Web.Controllers;

using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SportData.Data.Contexts;
using SportData.Data.ViewModels;
using SportData.Data.ViewModels.Home;

public class HomeController : BaseController
{
    private readonly SportDataDbContext context;

    public HomeController(ILogger<BaseController> logger, SportDataDbContext context)
        : base(logger)
    {
        this.context = context;
    }

    public IActionResult Index()
    {
        var users = this.context.Users.Count();
        this.ViewData["users"] = users;
        this.ViewBag.Count = 9;
        var viewModel = new HomeViewModel
        {
            Id = 1,
            Name = "Test",
            Description = "Description asdasda sdasdasdasdasdasdad",
        };

        return this.View(viewModel);
    }

    public IActionResult Privacy()
    {
        return this.View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}