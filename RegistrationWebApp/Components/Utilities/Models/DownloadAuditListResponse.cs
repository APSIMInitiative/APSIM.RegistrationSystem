namespace RegistrationWebApp.Components.Utilities.Models;

public class DownloadAuditListResponse
{
    public int Total { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; }

    public List<DownloadAuditResponse> Items { get; set; } = new();
}
