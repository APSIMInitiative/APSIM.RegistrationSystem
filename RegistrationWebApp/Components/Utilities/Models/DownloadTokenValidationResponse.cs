namespace RegistrationWebApp.Components.Utilities.Models;

public class DownloadTokenValidationResponse
{
    public bool IsValid { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
