namespace RegistrationWebApp.Components.Utilities.Models;

public class DownloadEventRequest
{
    public string Token { get; set; } = string.Empty;

    public string DownloadType { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string? Platform { get; set; }

    public string? DownloadUrl { get; set; }
}
