using System.Text.Json;
using RegistrationWebApp.Components.Utilities.Models;
using System.Net.Http.Headers;
using RegistrationShared.Models;
using System.Net;
using RegistrationShared.Enums;
using System.Globalization;

namespace RegistrationWebApp.Components.Utilities;

public partial class WebApiUtility
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly string? _configuredUsername;
    private readonly string? _configuredPassword;

    private string AuthenticationToken { get; set; } = string.Empty;

    private const string AuthTokenEndpoint = "api/auth/token";
    private const string UsersEndpoint = "api/users";
    private const string UserVerificationEndpoint = "api/users/verify";
    private const string OrganisationsEndpoint = "api/organisations";
    private const string DownloadLinkEndpoint = "api/downloads/link";
    private const string DownloadTokenValidationEndpoint = "api/downloads/validate";
    private const string DownloadEventEndpoint = "api/downloads/events";
    private const string DownloadEventExportEndpoint = "api/downloads/events/export";

    /// <summary>The name of the environment variable that can be used to 
    /// override the web API base URL configured in appsettings.json.</summary>
    private const string WebApiUrlEnvironmentVariable = "WEB_API_URL";

    /// <summary>The base URL for the web API, which can be set via 
    /// configuration or overridden by an environment variable.</summary>
    private string? configuredBaseUrl;

    /// <summary> The username for authenticating with the web API, 
    /// which can be set via configuration or overridden by 
    /// an environment variable.</summary>
    private string? AuthenticationUsername { get; set; }

    /// <summary> The password for authenticating with the web API, 
    /// which can be set via configuration or overridden 
    /// by an environment variable.</summary>
    private string? AuthenticationPassword { get; set; }

    /// <summary>
    /// The name of the environment variable that can be used to set the username for authenticating with the web API.
    /// </summary>
    private const string AuthenticationUsernameEnvironmentVariable = "WEB_API_USERNAME";

    /// <summary> The name of the environment variable that can be used to set the password for authenticating with the web API.</summary>
    private const string AuthenticationPasswordEnvironmentVariable = "WEB_API_PASSWORD";

    public WebApiUtility(IConfiguration configuration)
    {
        Configure(
            configuration["WebApi:BaseUrl"],
            configuration["WebApi:Username"],
            configuration["WebApi:Password"]);

        _baseUrl = GetBaseUrl();
        _configuredUsername = AuthenticationUsername;
        _configuredPassword = AuthenticationPassword;
        _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    /// <summary> Configures the web API utility with a base URL from configuration. 
    /// This can be overridden by setting the WEB_API_URL environment variable.
    /// </summary>
    /// <param name="baseUrl">The base URL from configuration.</param>
    /// <param name="username">The username for authenticating with the web API.</param>
    /// <param name="password">The password for authenticating with the web API.</param>
    public void Configure(string? baseUrl, string? username = null, string? password = null)
    {
        configuredBaseUrl = baseUrl;

        if (!string.IsNullOrEmpty(username))
        {
            AuthenticationUsername = username;
        }
        else
        {
            AuthenticationUsername = GetValueFromEnvironmentVariable(AuthenticationUsernameEnvironmentVariable);
        }

        if (!string.IsNullOrEmpty(password))
        {
            AuthenticationPassword = password;
        }
        else
        {
            AuthenticationPassword = GetValueFromEnvironmentVariable(AuthenticationPasswordEnvironmentVariable);
        }
    }

    /// <summary> Gets the authentication username from the environment variable.
    /// </summary>
    /// <param name="envName">The name of the environment variable.</param>
    /// <returns>The value of the environment variable, or null if not set.</returns>
    private static string? GetValueFromEnvironmentVariable(string envName)
    {
        return Environment.GetEnvironmentVariable(envName);
    }

    /// <summary> Gets the web API base URL from configuration (appsettings.json). 
    /// This is used if the environment variable is not set.
    /// </summary>
    /// <returns>Configuration string value or null if not set.</returns>

    public string? GetBaseUrlFromConfiguration()
    {
        return configuredBaseUrl;
    }

    /// <summary> Gets the web API base URL, preferring the environment variable over configuration.
    /// Throws an exception if neither is set.
    /// </summary>
    /// <returns>The web API base URL.</returns>
    public string GetBaseUrl()
    {
        return GetValueFromEnvironmentVariable(WebApiUrlEnvironmentVariable)
            ?? GetBaseUrlFromConfiguration()
            ?? throw new InvalidOperationException("A web API base URL must be configured via the WEB_API_URL environment variable or the WebApi:BaseUrl configuration setting.");
    }

    /// <summary>
    /// Constructs a full endpoint URL by combining the base URL with the specified endpoint path.
    /// </summary>
    /// <param name="endpoint">The endpoint path to append to the base URL.</param>
    /// <returns>The full URL for the specified endpoint.</returns>
    public string GetEndpointUrl(string endpoint)
    {
        return new Uri(new Uri(_baseUrl), endpoint).ToString();
    }

    /// <summary> Gets an authentication token from the web API using the configured username and password.
    /// This method sends a request to the authentication endpoint of the web API and retrieves a JWT token that can be used for authenticated requests.
    /// </summary> <returns>A JWT token string that can be used for authenticating requests to the web API.</returns>
    public async Task<string> GetAuthenticationToken()
    {
        if (!string.IsNullOrWhiteSpace(AuthenticationToken))
        {
            return AuthenticationToken;
        }

        var username = _configuredUsername
            ?? throw new InvalidOperationException("Authentication username is not configured.");
        var password = _configuredPassword
            ?? throw new InvalidOperationException("Authentication password is not configured.");

        string authenticationEndpoint = GetEndpointUrl(AuthTokenEndpoint);
        Login login = new(username, password);
        string body = JsonSerializer.Serialize(login);
        HttpResponseMessage response = await _client.PostAsync(authenticationEndpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        using var jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AuthenticationToken = jsonDocument.RootElement.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Authentication response did not include an access token.");
        return AuthenticationToken;
    }


    /// <summary>
    /// Authenticates an HTTP request by adding a Bearer token to the Authorization header.
    /// </summary>
    /// <param name="client">The HttpClient instance to authenticate.</param>
    /// <param name="token">The Bearer token to use for authentication.</param>
    public static void AuthenticateRequest(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Retrieves a list of users from the web API.
    /// </summary>
    public async Task<List<User>> GetUsersAsync()
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        HttpResponseMessage response = await _client.GetAsync(GetEndpointUrl(UsersEndpoint));
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<User>>(content, JsonOptions) ?? new List<User>();
    }

    /// <summary>
    /// Creates a new user by sending a POST request to the web API.
    /// </summary>
    /// <param name="user">The user object to create.</param>
    public async Task<UserResponseModel> CreateUserAsync(User user)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string body = JsonSerializer.Serialize(user);
        HttpResponseMessage response = await _client.PostAsync(GetEndpointUrl(UsersEndpoint),
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new UserResponseModel { Message = "A user with this email already exists.", User = null };
        }
        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();
        User? newUser = JsonSerializer.Deserialize<User>(responseContent, JsonOptions);
        string messageString = "We've sent you a verification email. " +
            "Click the link to download APSIM. If you don't see it, check your spam or junk folder.";
        return new UserResponseModel { Message = messageString, User = newUser };
    }

    /// <summary>
    /// Deletes a user by id by sending a DELETE request to the web API.
    /// </summary>
    /// <param name="userId">The id of the user to delete.</param>
    public async Task<HttpResponseMessage> DeleteUserAsync(Guid userId)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        return await _client.DeleteAsync(GetEndpointUrl($"{UsersEndpoint}/{userId}"));
    }


    /// <summary>
    /// Checks whether a user with the given email exists in the system.
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    public async Task<bool> CheckRegistrationEmailAsync(string email)
    {
        var users = await GetUsersAsync();
        return users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates a download access token with the web API.
    /// </summary>
    /// <param name="token">The token from the download page query string.</param>
    public async Task<DownloadTokenValidationResponse?> ValidateDownloadTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        string endpoint = GetEndpointUrl($"{DownloadTokenValidationEndpoint}?token={Uri.EscapeDataString(token)}");
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DownloadTokenValidationResponse>(content, JsonOptions);
    }

    /// <summary>
    /// Requests a download link redirect URL from the API for an existing registered user.
    /// </summary>
    /// <param name="email">The email address to validate for download access.</param>
    public async Task<bool> GetDownloadRedirectLinkAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        string endpoint = GetEndpointUrl($"{DownloadLinkEndpoint}?email={Uri.EscapeDataString(email.Trim())}");

        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync(endpoint);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Verifies a user email token and returns a WebApp download URL when verification succeeds.
    /// </summary>
    /// <param name="token">The user verification token from the query string.</param>
    public async Task<string?> VerifyUserAndGetDownloadUrlAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        string endpoint = GetEndpointUrl($"{UserVerificationEndpoint}?token={Uri.EscapeDataString(token)}");
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument jsonDocument = JsonDocument.Parse(content);
        if (!jsonDocument.RootElement.TryGetProperty("downloadUrl", out JsonElement downloadUrlElement))
        {
            return null;
        }

        return downloadUrlElement.GetString();
    }

    /// <summary>
    /// Verifies an organisation email token.
    /// </summary>
    /// <param name="token">The organisation verification token.</param>
    /// <param name="payload">The protected organisation verification payload.</param>
    public async Task<bool> VerifyOrganisationAsync(string token, string payload)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        string endpoint = GetEndpointUrl($"{OrganisationsEndpoint}/verify?token={Uri.EscapeDataString(token)}&payload={Uri.EscapeDataString(payload)}");
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Records a download event for auditing via the web API.
    /// </summary>
    public async Task<bool> RecordDownloadEventAsync(DownloadEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.DownloadType) ||
            string.IsNullOrWhiteSpace(request.Version))
        {
            return false;
        }

        string endpoint = GetEndpointUrl(DownloadEventEndpoint);
        string body = JsonSerializer.Serialize(request);

        HttpResponseMessage response = await _client.PostAsync(endpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Gets download event rows from the API with optional filtering and paging.
    /// </summary>
    public async Task<DownloadAuditListResponse?> GetDownloadEventsAsync(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? email = null,
        string? downloadType = null,
        int skip = 0,
        int take = 100)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);

        string queryString = BuildDownloadEventsQueryString(fromUtc, toUtc, email, downloadType, skip, take);
        string endpoint = GetEndpointUrl($"{DownloadEventEndpoint}{queryString}");
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DownloadAuditListResponse>(content, JsonOptions);
    }

    /// <summary>
    /// Gets only the total number of download events matching a filter.
    /// </summary>
    public async Task<int> GetDownloadEventsCountAsync(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? email = null,
        string? downloadType = null)
    {
        var response = await GetDownloadEventsAsync(fromUtc, toUtc, email, downloadType, skip: 0, take: 1);
        return response?.Total ?? 0;
    }

    /// <summary>
    /// Exports filtered download events as CSV.
    /// </summary>
    public async Task<DownloadCsvExportResult?> ExportDownloadEventsCsvAsync(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? email = null,
        string? downloadType = null)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);

        string queryString = BuildDownloadEventsQueryString(fromUtc, toUtc, email, downloadType);
        string endpoint = GetEndpointUrl($"{DownloadEventExportEndpoint}{queryString}");
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        byte[] bytes = await response.Content.ReadAsByteArrayAsync();
        string fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? $"download-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return new DownloadCsvExportResult
        {
            FileName = fileName.Trim('"'),
            ContentType = response.Content.Headers.ContentType?.MediaType ?? "text/csv",
            Bytes = bytes
        };
    }

    private static string BuildDownloadEventsQueryString(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? email,
        string? downloadType,
        int? skip = null,
        int? take = null)
    {
        var queryParts = new List<string>();

        if (fromUtc.HasValue)
        {
            queryParts.Add($"fromUtc={Uri.EscapeDataString(fromUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (toUtc.HasValue)
        {
            queryParts.Add($"toUtc={Uri.EscapeDataString(toUtc.Value.ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            queryParts.Add($"email={Uri.EscapeDataString(email.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(downloadType))
        {
            queryParts.Add($"downloadType={Uri.EscapeDataString(downloadType.Trim())}");
        }

        if (skip.HasValue)
        {
            queryParts.Add($"skip={skip.Value}");
        }

        if (take.HasValue)
        {
            queryParts.Add($"take={take.Value}");
        }

        return queryParts.Count == 0 ? string.Empty : $"?{string.Join("&", queryParts)}";
    }

    /// <summary>
    /// Adds a new member organisation by sending a POST request to the 
    /// member organisations endpoint of the web API with the member 
    /// organisation data serialized in the request body.
    /// </summary>
    /// <param name="organisation">The organisation object to be added.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    /// <exception cref="Exception">Thrown when the request fails or the response indicates an error.</exception>
    public async Task<HttpResponseMessage> AddOrganisationAsync(Organisation organisation)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string body = JsonSerializer.Serialize(organisation);
        string endpoint = GetEndpointUrl(OrganisationsEndpoint);
        HttpResponseMessage response = await _client.PostAsync(endpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        return response;
    }

    /// <summary>
    /// Updates an existing organisation by sending a PUT request to the web API.
    /// </summary>
    /// <param name="organisation">The organisation object containing updated values.</param>
    /// <returns>The HTTP response message from the API.</returns>
    public async Task<HttpResponseMessage> UpdateOrganisationAsync(Organisation organisation)
    {
        if (organisation.Id == Guid.Empty)
        {
            throw new ArgumentException("Organisation Id is required for updates.", nameof(organisation));
        }

        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string body = JsonSerializer.Serialize(organisation);
        string endpoint = GetEndpointUrl($"{OrganisationsEndpoint}/{organisation.Id}");
        HttpResponseMessage response = await _client.PutAsync(endpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        return response;
    }



    /// <summary>
    /// Retrieves a list of member organisations from the web API.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of Organisation objects.</returns>
    public async Task<List<Organisation>> GetOrganisationsAsync()
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string endpoint = GetEndpointUrl(OrganisationsEndpoint);
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        var memberOrganisations = JsonSerializer.Deserialize<List<Organisation>>(content, JsonOptions) ?? new List<Organisation>();
        return memberOrganisations;
    }

    /// <summary>
    /// Retrieves a list of member organisation names from the web API by first getting the list of member organisations and then selecting their names.
    /// </summary>
    /// <returns>The task result contains a list of member organisation names.</returns>
    public async Task<List<string>> GetMemberOrganisationNamesAsync()
    {
        List<Organisation> Organisations = await GetOrganisationsAsync();
        return Organisations.Where(o => o.LicencePathway == LicencePathway.APSIMInitiativeMember).Select(o => o.Name).ToList();
    }

    /// <summary>
    /// Retrieves names of organisations with a Special Use licence pathway.
    /// </summary>
    public async Task<List<string>> GetSpecialUseOrganisationNames()
    {
        var organisations = await GetOrganisationsAsync();
        return organisations
            .Where(o => o.LicencePathway == LicencePathway.TypeOne || o.LicencePathway == LicencePathway.TypeTwo)
            .Select(o => o.Name)
            .ToList();
    }

    /// <summary>
    /// Retrieves a combined list of member organisation names and 
    /// special use organisation names by calling the respective methods to 
    /// get each list and then concatenating the results.
    /// </summary>
    /// <returns>The task result contains a list of organisation names.</returns>
    public async Task<List<string>> GetMemberOrgAndSpecialUseOrgNames()
    {
        List<string> memberOrgNames = await GetMemberOrganisationNamesAsync();
        List<string> specialUseOrgNames = await GetSpecialUseOrganisationNames();
        return memberOrgNames.Concat(specialUseOrgNames).ToList();
    }

    /// <summary>
    /// Gets the country string for a provided IP Address.
    /// </summary>
    /// <param name="ipaddress"></param>
    /// <returns>The name of a country</returns>
    /// <exception cref="InvalidDataException"></exception>
    public async Task<string> GetCountryNameFromIPAddress(string ipaddress)
    {
        string defaultCountryName = "unavailable";
        if (string.IsNullOrEmpty(ipaddress))
            throw new InvalidDataException("Error: An empty IP address was provided.");
        // Query the IPInfo API.
        HttpClient outsideClient = new();
        string url = $"https://api.ipinfo.io/lite/{ipaddress}";
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "4640501e1e3c57");
        using HttpResponseMessage response = await outsideClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string jsonString = await response.Content.ReadAsStringAsync();
        Dictionary<string,object> ipInfoContent = JsonSerializer.Deserialize<Dictionary<string,object>>(jsonString) ?? 
            new Dictionary<string, object>();

        if (ipInfoContent.TryGetValue("country", out object? value))
            return value.ToString() ?? defaultCountryName;
        else return defaultCountryName;
    }

}
