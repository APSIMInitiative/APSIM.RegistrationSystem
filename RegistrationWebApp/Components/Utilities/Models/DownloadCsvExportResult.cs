namespace RegistrationWebApp.Components.Utilities.Models;

public class DownloadCsvExportResult
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "text/csv";

    public byte[] Bytes { get; set; } = Array.Empty<byte>();
}
