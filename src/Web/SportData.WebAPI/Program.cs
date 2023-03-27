namespace SportData.WebAPI;

using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using SportData.Common.Constants;
using SportData.Data.Contexts;
using SportData.Data.Models.Entities.Users;
using SportData.WebAPI.Infrastructure.Filters.Swagger;

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

        // Users Db
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(AppGlobalConstants.USERS_CONNECTION_STRING)).UseLazyLoadingProxies();
        });

        // Config identity
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 0;
        });

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration.GetValue<string>(AppGlobalConstants.JWT_AUDIENCE),
                ValidIssuer = configuration.GetValue<string>(AppGlobalConstants.JWT_ISSUER),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>(AppGlobalConstants.JWT_SECRET)))
            };
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "SportData123", Version = "v1" });
            options.SchemaFilter<SwaggerSchemaExampleFilter>();
            options.OperationFilter<SwaggerTestOperationFilter>();
        });
    }

    private static void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}