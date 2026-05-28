namespace RegistrationWebAPI.Models;

public class DownloadAuditResponse
{
    public Guid Id { get; set; }

    public DateTime DownloadedAtUtc { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public Guid? UserId { get; set; }

    public string DownloadType { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string? Platform { get; set; }

    public string? DownloadUrl { get; set; }
}
