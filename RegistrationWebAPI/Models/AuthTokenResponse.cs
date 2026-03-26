namespace RegistrationWebAPI.Models;

public class AuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
