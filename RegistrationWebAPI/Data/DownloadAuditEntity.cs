using System.ComponentModel.DataAnnotations;

namespace RegistrationWebAPI.Data;

public class DownloadAuditEntity
{
    public Guid Id { get; set; }

    public DateTime DownloadedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserEmail { get; set; } = string.Empty;

    public Guid? UserId { get; set; }

    [Required]
    public string DownloadType { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;

    public string? Platform { get; set; }

    public string? DownloadUrl { get; set; }
}
