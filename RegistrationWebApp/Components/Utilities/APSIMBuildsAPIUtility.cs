using System.Text.Json;
using RegistrationWebApp.Components.Utilities.Models;

namespace RegistrationWebApp.Components.Utilities;

public class APSIMBuildsAPIUtility
{
    private readonly HttpClient _httpClient;

    private const string BuildsApiUrl = "https://builds.apsim.info/api";

    public const string ClassicBuildsDownloadURL = "/oldapsim/download/";

    public APSIMBuildsAPIUtility(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the latest next gen APSIM build information from the API.
    /// </summary>
    /// <exception cref="HttpRequestException">Throws if no build information is found.</exception>
    /// <returns>A Task that represents the asynchronous operation. 
    /// The task result contains the latest next gen APSIM build information.</returns>
    public async Task<List<APSIMNextGenDownloadInfo>?> GetAPSIMNextGenBuildsListAsync()
    {
        try
        {
            string endpoint = "/nextgen/list";
            string url = $"{BuildsApiUrl}{endpoint}";
            HttpResponseMessage response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
                throw new HttpRequestException("Received empty response from APSIM Builds API when fetching latest next gen builds.");
            return JsonSerializer.Deserialize<List<APSIMNextGenDownloadInfo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<APSIMNextGenDownloadInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching latest next gen builds: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets a list of APSIM Classic builds from the APSIM Builds API.
    /// </summary>
    /// <returns>A list of APSIM Classic build information.</returns>
    /// <exception cref="HttpRequestException">Throws if no build information is found.</exception>
    public async Task<List<APSIMClassicDownloadInfo>?> GetAPSIMClassicBuildsListAsync()
    {
        try
        {
            string endpoint = $"/oldapsim/list";
            string url = $"{BuildsApiUrl}{endpoint}";
            HttpResponseMessage response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
                throw new HttpRequestException("Received empty response from APSIM Builds API when fetching APSIM Classic builds.");
            return JsonSerializer.Deserialize<List<APSIMClassicDownloadInfo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<APSIMClassicDownloadInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching APSIM Classic builds: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Constructs the download link for a specific APSIM Classic build based on the provided revision number.
    /// </summary>
    /// <param name="revisionNumber"></param>
    /// <returns></returns>
    public string GetClassicBuildDownloadLink(int revisionNumber)
    {
        return $"{BuildsApiUrl}{ClassicBuildsDownloadURL}{revisionNumber}";
    }
}