using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SportData.Common.Constants;
using SportData.Data.Contexts;
using SportData.Web.Infrastructure.Filters;
using SportData.Web.Services;

namespace SportData.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);
        var app = builder.Build();
        Configure(app);
        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Logging
        services.AddLogging(config =>
        {
            config.AddConfiguration(configuration.GetSection(AppGlobalConstants.LOGGING));
            config.AddConsole();
            config.AddLog4Net(configuration.GetSection(AppGlobalConstants.LOG4NET_CORE).Get<Log4NetProviderOptions>());
        });

        // Add services to the container.
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<UserDbContext>();

        services.AddRazorPages();
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new CustomActionFilterAttribute());
            options.Filters.Add(new CustomAuthorizationFilterAttribute());
            options.Filters.Add(new CustomExceptionFilterAttribute());
            options.Filters.Add(new CustomResourceFilterAttribute());
            options.Filters.Add(new CustomResultFilterAttribute());
        }).AddRazorRuntimeCompilation();

        services.AddScoped<IShortStringService, ShortStringService>();
    }

    private static void Configure(WebApplication app)
    {
        // ip limit middleware ????????? nuget
        //app.UseMiddleware<CustomeMiddleware>();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();

            //app.UseStatusCodePages();
            //app.UseStatusCodePagesWithRedirects();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseCookiePolicy();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllerRoute("area", "{area:exist}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();
    }
}