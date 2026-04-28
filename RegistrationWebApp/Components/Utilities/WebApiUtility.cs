using System.Text.Json;
using RegistrationWebApp.Components.Utilities.Models;
using RegistrationShared.Interfaces;
using System.Net.Http.Headers;
using RegistrationShared.Models;
using System.Net;
using RegistrationWebApp.Components.Classes;

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
    private const string RegistrationEndpoint = "api/registrations";
    private const string MemberOrganisationsEndpoint = "api/member-organisations";

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
    /// <remarks> Will never return null. If there are no registrations, 
    /// an empty list will be returned.</remarks>
    /// <returns>A list of IRegistration objects representing the 
    /// registrations retrieved from the web API.</returns>
    public async Task<List<IRegistration>> GetRegistrationsAsync()
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        HttpResponseMessage response = await _client.GetAsync(RegistrationEndpoint);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        var apiRegistrations = JsonSerializer.Deserialize<List<RegistrationApiModel>>(content, JsonOptions) ?? new List<RegistrationApiModel>();

        return apiRegistrations
            .Select(MapApiRegistration)
            .ToList();
    }

    private static IRegistration MapApiRegistration(RegistrationApiModel registration)
    {
        if (registration.IsSpecialUse())
        {
            return new SpecialUseRegistration
            {
                ContactName = registration.ContactName,
                ContactEmail = registration.ContactEmail,
                ApplicationDate = registration.ApplicationDate,
                LicenceStatus = registration.LicenceStatus,
                OrganisationName = registration.OrganisationName,
                OrganisationAddress = registration.OrganisationAddress,
                OrganisationWebsite = registration.OrganisationWebsite,
                ContactPhone = registration.ContactPhone,
                LicencePathWay = registration.LicencePathway ?? default,
                AnnualTurnover = registration.AnnualTurnover ?? default,
                AgreesToTerms = registration.AgreesToTerms ?? false,
            };
        }

        return new GeneralUseRegistration
        {
            ContactName = registration.ContactName,
            ContactEmail = registration.ContactEmail,
            ApplicationDate = registration.ApplicationDate,
            LicenceStatus = registration.LicenceStatus,
            AgreesToTerms = registration.AgreesToTerms ?? false,
        };
    }

    public async Task<ResponseModel> CreateRegistrationAsync(IRegistration registration)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string body = GetRegistrationBody(registration);
        HttpResponseMessage response = await _client.PostAsync(RegistrationEndpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new ResponseModel { Message = "A registration with this email already exists." };
        }
        response.EnsureSuccessStatusCode();

        // Try to deserialize the response into a GeneralUseRegistration first, then a SpecialUseRegistration if that fails.
        string responseContent = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseContent);
        IRegistration? registrationModel;
        if (jsonDocument.RootElement.GetProperty("organisationName").ValueKind != JsonValueKind.Null)      
        {
            registrationModel = JsonSerializer.Deserialize<SpecialUseRegistration>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else registrationModel = JsonSerializer.Deserialize<GeneralUseRegistration>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return new ResponseModel { Message = "Registration successful.", Registration = registrationModel };
    }

    /// <summary>
    /// Constructs the request body for creating a registration by serializing the appropriate registration model based on the licence status.
    /// </summary>
    /// <param name="registration">The registration object to be serialized.</param>
    /// <returns>A JSON string representing the registration object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the registration type is invalid.</exception>
    private static string GetRegistrationBody(IRegistration registration)
    {
        object requestModel;

        if (registration is SpecialUseRegistration specialUseReg)
        {
            requestModel = new
            {
                registrationType = 1,  // RegistrationType.SpecialUse = 1
                contactName = specialUseReg.ContactName,
                contactEmail = specialUseReg.ContactEmail,
                applicationDate = specialUseReg.ApplicationDate,
                licenceStatus = specialUseReg.LicenceStatus,
                organisationName = specialUseReg.OrganisationName,
                organisationAddress = specialUseReg.OrganisationAddress,
                organisationWebsite = specialUseReg.OrganisationWebsite,
                contactPhone = specialUseReg.ContactPhone,
                licencePathway = specialUseReg.LicencePathWay,
                annualTurnover = specialUseReg.AnnualTurnover,
                agreesToTerms = true,
            };
        }
        else if (registration is GeneralUseRegistration generalUseReg)
        {
            requestModel = new
            {
                registrationType = 0,  // RegistrationType.GeneralUse = 0
                contactName = generalUseReg.ContactName,
                contactEmail = generalUseReg.ContactEmail,
                applicationDate = generalUseReg.ApplicationDate,
                licenceStatus = generalUseReg.LicenceStatus,
                agreesToTerms = true,
            };
        }
        else if (registration is MemberOrganisationRegistration memberOrgReg)
        {
            requestModel = new
            {
                registrationType = 2,  // RegistrationType.MemberOrganisation = 2
                contactName = memberOrgReg.ContactName,
                contactEmail = memberOrgReg.ContactEmail,
                applicationDate = memberOrgReg.ApplicationDate,
                licenceStatus = memberOrgReg.LicenceStatus,
                agreesToTerms = memberOrgReg.AgreesToTerms,
            };
        }
        else if (registration is SpecialUseStaffRegistration specialUseStaffReg)
        {
            requestModel = new
            {
                registrationType = 3,  // RegistrationType.SpecialUseStaff = 3
                contactName = specialUseStaffReg.ContactName,
                contactEmail = specialUseStaffReg.ContactEmail,
                applicationDate = specialUseStaffReg.ApplicationDate,
                licenceStatus = specialUseStaffReg.LicenceStatus,
                agreesToTerms = specialUseStaffReg.AgreesToTerms,
            };
        }
        else
        {
            throw new InvalidOperationException("Invalid registration type.");
        }

        return JsonSerializer.Serialize(requestModel);
    }

    /// <summary>
    /// Checks if a registration with the specified email already exists by retrieving the list of registrations from the web API and checking for a matching contact email.
    /// </summary>
    /// <param name="email">The email address to check for an existing registration.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a boolean indicating whether a registration with the specified email exists.</returns>
    public async Task<bool> CheckRegistrationEmailAsync(string email)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string endpoint = $"{RegistrationEndpoint}?contactEmail={email}";
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        var registrationsJson = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(content, JsonOptions);
        return registrationsJson?.FirstOrDefault(r => r["contactEmail"].GetString() == email) != null;
    }

    /// <summary>
    /// Adds a new member organisation by sending a POST request to the 
    /// member organisations endpoint of the web API with the member 
    /// organisation data serialized in the request body.
    /// </summary>
    /// <param name="memberOrganisation">The member organisation object to be added.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    public async Task<HttpResponseMessage> AddMemberOrganisationAsync(MemberOrganisation memberOrganisation)
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string body = JsonSerializer.Serialize(memberOrganisation);
        string endpoint = GetEndpointUrl(MemberOrganisationsEndpoint);
        HttpResponseMessage response = await _client.PostAsync(endpoint,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        return response;
    }

    /// <summary>
    /// Retrieves a list of member organisations from the web API.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains a list of MemberOrganisation objects.</returns>
    public async Task<List<MemberOrganisation>> GetMemberOrganisationsAsync()
    {
        string token = await GetAuthenticationToken();
        AuthenticateRequest(_client, token);
        string endpoint = GetEndpointUrl(MemberOrganisationsEndpoint);
        HttpResponseMessage response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        var memberOrganisations = JsonSerializer.Deserialize<List<MemberOrganisation>>(content, JsonOptions) ?? new List<MemberOrganisation>();
        return memberOrganisations;
    }
}
