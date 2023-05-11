namespace SportData.Data.Models.Authentication;

public class AuthenticationRequestModel
{
    /// <summary>
    /// Client username's
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Client password's
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; }
}