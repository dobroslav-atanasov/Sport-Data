namespace SportData.Data.Models.Authentication;

using Swashbuckle.AspNetCore.Annotations;

[SwaggerSchema(Description = "Token model")]
public class TokenModel
{
    [SwaggerSchema(Description = "This is test")]
    public string AccessToken { get; set; }

    /// <summary>
    /// Refresh token
    /// </summary>
    /// <example>asd</example>
    public string RefreshToken { get; set; }
}