using System.Net.Http;
using System.Text.Json;
using RegistrationWebApp.Components.Utilities.Models;
using RegistrationShared.Interfaces;
using System.Net.Http.Headers;

namespace RegistrationWebApp.Components.Utilities;

public static class WebApiUtility
{
    /// <summary>A static HttpClient instance configured with the base URL for the web API.</summary>
    private static HttpClient? _client;

    /// <summary>Gets the HttpClient instance, initializing it if necessary. 
    /// The HttpClient is configured with the base URL for the web API, which 
    /// can be set via an environment variable or configuration. 
    /// If the base URL changes, the HttpClient will be reconfigured to use 
    /// the new base URL.</summary>
    private static HttpClient Client { 
        get
        {
            if (_client == null)
            {
                _client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) };
            }
            return _client;

        }
        set{
            // Ensure the HttpClient has the correct base address set
            value.BaseAddress = new Uri(GetBaseUrl());
            Client = value;
        } 
    }

    private static string AuthenticationToken { get; set; } = string.Empty;

    private const string AuthTokenEndpoint = "api/auth/token";
    private const string RegistrationEndpoint = "api/registrations";

    /// <summary>The name of the environment variable that can be used to 
    /// override the web API base URL configured in appsettings.json.</summary>
    private const string WebApiUrlEnvironmentVariable = "WEB_API_URL";

    /// <summary>The base URL for the web API, which can be set via 
    /// configuration or overridden by an environment variable.</summary>
    private static string? configuredBaseUrl;

    /// <summary> The username for authenticating with the web API, 
    /// which can be set via configuration or overridden by 
    /// an environment variable.</summary>
    private static string? AuthenticationUsername { get; set; }

    /// <summary> The password for authenticating with the web API, 
    /// which can be set via configuration or overridden 
    /// by an environment variable.</summary>
    private static string? AuthenticationPassword { get; set; }

    /// <summary>
    /// /// The name of the environment variable that can be used to set the username for authenticating with the web API.
    /// </summary>
    private const string AuthenticationUsernameEnvironmentVariable = "WEB_API_USERNAME";

    /// <summary> The name of the environment variable that can be used to set the password for authenticating with the web API.</summary>
    private const string AuthenticationPasswordEnvironmentVariable = "WEB_API_PASSWORD";

    /// <summary> Configures the web API utility with a base URL from configuration. 
    /// This can be overridden by setting the WEB_API_URL environment variable.
    /// </summary>
    /// <param name="baseUrl">The base URL from configuration.</param>
    /// <param name="username">The username for authenticating with the web API.</param>
    /// <param name="password">The password for authenticating with the web API.</param>
    public static void Configure(string? baseUrl, string? username = null, string? password = null)
    {
        configuredBaseUrl = baseUrl;

        if (username != null)
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
    private static string? GetValueFromEnvironmentVariable(string envName)
    {
        return Environment.GetEnvironmentVariable(envName);
    }

    /// <summary> Gets the web API base URL from configuration (appsettings.json). 
    /// This is used if the environment variable is not set.
    /// </summary>
    /// <returns>Configuration string value or null if not set.</returns>

    public static string? GetBaseUrlFromConfiguration()
    {
        return configuredBaseUrl;
    }

    /// <summary> Gets the web API base URL, preferring the environment variable over configuration.
    /// Throws an exception if neither is set.
    /// </summary>
    /// <returns>The web API base URL.</returns>
    public static string GetBaseUrl()
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
    public static string GetEndpointUrl(string endpoint)
    {
        return new Uri(new Uri(GetBaseUrl()), endpoint).ToString();
    }

    /// <summary> Gets an authentication token from the web API using the configured username and password.
    /// This method sends a request to the authentication endpoint of the web API and retrieves a JWT token that can be used for authenticated requests.
    /// </summary> <returns>A JWT token string that can be used for authenticating requests to the web API.</returns>
    public static async Task<string> GetAuthenticationToken()
    {
        string authenticationEndpoint = GetEndpointUrl("api/auth/token");
        Login login = new(AuthenticationUsername ?? throw new InvalidOperationException("Authentication username is not configured."), 
            AuthenticationPassword ?? throw new InvalidOperationException("Authentication password is not configured."));
        string body = JsonSerializer.Serialize(login);
        HttpResponseMessage response = await Client.PostAsync(authenticationEndpoint, 
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
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
    public static async Task<List<IRegistration>> GetRegistrationsAsync()
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(Client, token);
        HttpResponseMessage response = await Client.GetAsync(RegistrationEndpoint);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer
            .Deserialize<List<IRegistration>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?
            .Cast<IRegistration>()
            .ToList() ?? new List<IRegistration>();
    }
}
