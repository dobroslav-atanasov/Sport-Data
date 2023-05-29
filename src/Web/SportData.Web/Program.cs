using Microsoft.EntityFrameworkCore;
namespace SportData.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SportData.Common.Constants;
using SportData.Data.Contexts;
using SportData.Data.Entities;
using SportData.Data.Options;
using SportData.Data.Seeders;
using SportData.Web.Infrastructure.Filters;
using SportData.Web.Infrastructure.Middlewares;

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
        services.AddDbContext<SportDataDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.SetIdentityOptions)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<SportDataDbContext>();

        services.AddRazorPages();
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new CustomActionFilterAttribute());
            options.Filters.Add(new CustomAuthorizationFilterAttribute());
            options.Filters.Add(new CustomExceptionFilterAttribute());
            options.Filters.Add(new CustomResourceFilterAttribute());
            options.Filters.Add(new CustomResultFilterAttribute());
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        }).AddRazorRuntimeCompilation();

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration.GetSection("Google:ClientId").Value;
                options.ClientSecret = configuration.GetSection("Google:ClientSecret").Value;
            });
    }

    private static void Configure(WebApplication app)
    {
        using (var serviceScope = app.Services.CreateScope())
        {
            var sportDataDbContext = serviceScope.ServiceProvider.GetRequiredService<SportDataDbContext>();
            sportDataDbContext.Database.Migrate();

            new SportDataDbSeeder().SeedAsync(serviceScope.ServiceProvider).GetAwaiter().GetResult();
        }

        // ip limit middleware ????????? nuget
        //app.UseMiddleware<CustomeMiddleware>();
        if (app.Environment.IsDevelopment())
        {
            app.UseMiddleware<ExceptionMiddleware>();
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

        app.MapControllerRoute("area", "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();
    }
}