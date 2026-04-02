
namespace RegistrationWebApp.Components.Utilities.Models;

/// <summary>Class representing the information about an APSIM 
/// next generation build that is retrieved from the APSIM Builds API.</summary>
public class APSIMNextGenDownloadInfo
{
        public DateTime ReleaseDate { get; set; }
        public int Issue { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DownloadLinkDebian { get; set; } = string.Empty;
        public string DownloadLinkWindows { get; set; } = string.Empty;
        public string DownloadLinkMacOS { get; set; } = string.Empty;
        public string InfoUrl { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int Revision { get; set; }

    /// <summary>
    /// Overrides the ToString method to provide a string representation 
    /// of the APSIMNextGenDownloadInfo object.
    /// </summary>
    /// <returns>A string representation of the APSIMNextGenDownloadInfo object.</returns>
    public override string ToString()
    {
        return $"Title: {Title}\n" + 
            $"Version: {Version}\n" +
            $"Revision: {Revision}\n" +
            $"ReleaseDate: {ReleaseDate}\n" +
            $"Issue: {Issue}\n" +
            $"InfoUrl: {InfoUrl}\n" +
            $"DownloadLinkDebian: {DownloadLinkDebian}\n" +
            $"DownloadLinkWindows: {DownloadLinkWindows}\n" +
            $"DownloadLinkMacOS: {DownloadLinkMacOS}";
    }
}