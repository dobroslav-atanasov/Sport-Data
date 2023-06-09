namespace SportData.Data.Options;

using Microsoft.AspNetCore.Identity;

public static class IdentityOptionsProvider
{
    public static void SetIdentityOptions(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedEmail = false;

        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequiredLength = 4;
        options.User.RequireUniqueEmail = true;
    }
}