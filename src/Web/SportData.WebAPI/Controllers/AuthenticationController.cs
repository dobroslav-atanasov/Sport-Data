namespace SportData.WebAPI.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportData.Data.Entities.SportData;
using SportData.Data.Models.Authentication;
using SportData.Data.Models.Entities.Enumerations;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> logger;
    private readonly IConfiguration configuration;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;

    public AuthenticationController(ILogger<AuthenticationController> logger, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.userManager = userManager;
        this.roleManager = roleManager;
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create(RegisterModel model)
    {
        var userExist = await this.userManager.FindByNameAsync(model.Username);
        if (userExist != null)
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = ResponseStatus.Error, Message = "User already exists!" });
        }

        var user = new ApplicationUser
        {
            Email = model.Email,
            UserName = model.Username,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        var result = await this.userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = ResponseStatus.Error, Message = "User can not be registered!" });
        }

        return this.Ok(new ResponseModel { Status = ResponseStatus.Success, Message = "User created successfully!" });
    }

    [HttpPost]
    [Route("Create-Admin")]
    public async Task<IActionResult> CreateAdmin(RegisterModel model)
    {
        return this.Ok();
    }

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var user = await this.userManager.FindByNameAsync(model.Username);
        if (user != null && await this.userManager.CheckPasswordAsync(user, model.Password))
        {
            return this.Ok();
        }

        return this.Unauthorized(new ResponseModel { Status = ResponseStatus.Error, Message = "Invalid usernam or password" });
    }

    [HttpPost]
    [Route("Refresh-Token")]
    public async Task<IActionResult> RefreshToken(TokenModel model)
    {
        return this.Ok();
    }

    [HttpPost]
    [Route("Revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        return this.Ok();
    }

    [HttpPost]
    [Route("Reveoke-All")]
    public async Task<IActionResult> RevokeAll()
    {
        return this.Ok();
    }
}