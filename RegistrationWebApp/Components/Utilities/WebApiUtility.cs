using System.Net.Http;
using System.Text.Json;
using RegistrationWebApp.Components.Utilities.Models;
using RegistrationShared.Interfaces;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace RegistrationWebApp.Components.Utilities;

public class WebApiUtility
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly string? _configuredUsername;
    private readonly string? _configuredPassword;

    private string AuthenticationToken { get; set; } = string.Empty;

    private const string AuthTokenEndpoint = "api/auth/token";
    private const string RegistrationEndpoint = "api/registrations";

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
    /// /// The name of the environment variable that can be used to set the username for authenticating with the web API.
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
    /// Get a list of registrations from the web API. 
    /// This method first retrieves an authentication token, then makes an 
    /// authenticated request to the registrations endpoint of the web API to 
    /// get the list of registrations. The response is deserialized into a list 
    /// of IRegistration objects and returned to the caller.
    /// </summary>
    /// <returns>A list of IRegistration objects representing the 
    /// registrations retrieved from the web API.</returns>
    public async Task<List<IRegistration>> GetRegistrationsAsync()
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        HttpResponseMessage response = await _client.GetAsync(RegistrationEndpoint);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer
            .Deserialize<List<IRegistration>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?
            .Cast<IRegistration>()
            .ToList() ?? new List<IRegistration>();
    }

    public async Task<string> CreateRegistrationAsync(IRegistration registration)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string body = JsonSerializer.Serialize(registration);
        HttpResponseMessage response = await _client.PostAsync(RegistrationEndpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
