using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RegistrationShared.Enums;
using RegistrationShared.Models;
using RegistrationWebAPI.Data;
using RegistrationWebAPI.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Tests.Utilities;

namespace Tests.RegistrationWebAPI;

public sealed class TestRegistrationWebAPI : IAsyncLifetime
{
    private readonly MockRegistrationWebAPI mockApi = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await mockApi.ResetDatabaseAsync();
        client = await mockApi.CreateAuthenticatedClientAsync();
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await mockApi.DisposeAsync();
    }

    // -------------------------------------------------------------------------
    // User CRUD
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser()
    {
        var user = new User { Email = "alice@example.com" };

        var response = await client.PostAsJsonAsync("/api/users", user);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(created);
        Assert.Equal(user.Email, created.Email);
        Assert.NotEqual(Guid.Empty, created.Id);
    }

    [Fact]
    public async Task VerifyUserEmail_UpdatesLicenceStatus()
    {
        var createResponse = await client.PostAsJsonAsync("/api/users", new User { Email = "verify-user@example.com" });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(created);

        string token;
        using (var scope = mockApi.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var entity = await db.Users.FirstAsync(x => x.Id == created.Id);
            Assert.NotNull(entity.EmailVerificationToken);
            token = entity.EmailVerificationToken!;
        }

        var verifyResponse = await client.GetAsync($"/api/users/verify?token={token}");
        verifyResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/json", verifyResponse.Content.Headers.ContentType?.MediaType);
        var responseBody = await verifyResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseBody);
        var downloadUrl = json.RootElement.GetProperty("downloadUrl").GetString();
        Assert.False(string.IsNullOrWhiteSpace(downloadUrl));
        Assert.StartsWith("http", downloadUrl, StringComparison.OrdinalIgnoreCase);

        using (var scope = mockApi.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var verified = await db.Users.FirstAsync(x => x.Id == created.Id);
            Assert.Equal(UserLicenceStatus.General, verified.LicenceStatus);
        }
    }

    [Fact]
    public async Task CreateUser_ReturnsValidationError_WhenEmailMissing()
    {
        var user = new User { Email = "" };

        var response = await client.PostAsJsonAsync("/api/users", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("email"));
    }

    [Fact]
    public async Task CreateUser_ReturnsValidationError_WhenEmailInvalid()
    {
        var user = new User { Email = "not-an-email" };

        var response = await client.PostAsJsonAsync("/api/users", user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("email"));
    }

    [Fact]
    public async Task CreateUser_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var user = new User { Email = "duplicate@example.com" };
        var first = await client.PostAsJsonAsync("/api/users", user);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/users", user);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task GetUser_ReturnsExistingUser()
    {
        var user = new User { Email = "bob@example.com" };
        var createResponse = await client.PostAsJsonAsync("/api/users", user);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/users/{created.Id}");
        getResponse.EnsureSuccessStatusCode();

        var fetched = await getResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(user.Email, fetched.Email);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_ForMissingId()
    {
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListUsers_ReturnsAllUsers()
    {
        await client.PostAsJsonAsync("/api/users", new User { Email = "user1@example.com" });
        await client.PostAsJsonAsync("/api/users", new User { Email = "user2@example.com" });

        var response = await client.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.True(users.Count >= 2);
    }

    [Fact]
    public async Task UpdateUser_ReturnsUpdatedUser()
    {
        var user = new User { Email = "update-me@example.com" };
        var createResponse = await client.PostAsJsonAsync("/api/users", user);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(created);

        created.LicenceStatus = UserLicenceStatus.General;
        var putResponse = await client.PutAsJsonAsync($"/api/users/{created.Id}", created);
        putResponse.EnsureSuccessStatusCode();

        var updated = await putResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(updated);
        Assert.Equal(UserLicenceStatus.General, updated.LicenceStatus);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_ForMissingId()
    {
        var user = new User { Email = "ghost@example.com" };
        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", user);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNoContent()
    {
        var user = new User { Email = "delete-me@example.com" };
        var createResponse = await client.PostAsJsonAsync("/api/users", user);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(created);

        var deleteResponse = await client.DeleteAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_ForMissingId()
    {
        var response = await client.DeleteAsync($"/api/users/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Organisation CRUD
    // -------------------------------------------------------------------------

    private static Organisation NewOrganisation(string name) => new()
    {
        Name = name,
        ContactName = "Test Contact",
        ContactEmail = "contact@example.com",
        ContactPhone = "0400000000",
        ContactAddress = "123 Test St",
        LicencePathway = LicencePathway.TypeOne,
        AnnualTurnover = AnnualTurnover.BelowTwoMillion,
    };

    [Fact]
    public async Task CreateOrganisation_ReturnsCreatedOrganisation()
    {
        var org = NewOrganisation("CSIRO");

        var response = await client.PostAsJsonAsync("/api/organisations", org);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(created);
        Assert.Equal(org.Name, created.Name);
        Assert.NotEqual(Guid.Empty, created.Id);

        using var scope = mockApi.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
        var persisted = await db.Organisations.FirstAsync(x => x.Id == created.Id);
        Assert.False(string.IsNullOrWhiteSpace(persisted.EmailVerificationToken));
        Assert.NotNull(persisted.EmailVerificationTokenExpiryUtc);
        Assert.True(persisted.EmailVerificationTokenExpiryUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task VerifyOrganisationEmail_UpdatesLicenceStatus()
    {
        var organisationRequest = NewOrganisation("VerifyOrg");
        var createResponse = await client.PostAsJsonAsync("/api/organisations", organisationRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(created);

        string token;
        string payload;
        using (var scope = mockApi.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var entity = await db.Organisations.FirstAsync(x => x.Id == created.Id);
            Assert.NotNull(entity.EmailVerificationToken);
            token = entity.EmailVerificationToken!;

            var protector = scope.ServiceProvider
                .GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("OrganisationVerificationPayload.v1");

            payload = protector.Protect(System.Text.Json.JsonSerializer.Serialize(new OrganisationVerificationPayload
            {
                OrganisationId = created.Id,
                OrganisationName = organisationRequest.Name,
                ContactName = organisationRequest.ContactName,
                ContactEmail = organisationRequest.ContactEmail,
                ContactPhone = organisationRequest.ContactPhone,
                ContactAddress = organisationRequest.ContactAddress,
                OrganisationEmails = organisationRequest.Emails,
                LicencePathway = organisationRequest.LicencePathway,
                AnnualTurnover = organisationRequest.AnnualTurnover,
                DateCreatedUtc = entity.DateCreated
            }));
        }

        var verifyResponse = await client.GetAsync($"/api/organisations/verify?token={Uri.EscapeDataString(token)}&payload={Uri.EscapeDataString(payload)}");
        verifyResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/json", verifyResponse.Content.Headers.ContentType?.MediaType);
        var responseBody = await verifyResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(responseBody);
        var isVerifiedResponse = json.RootElement.GetProperty("verified").GetBoolean();
        Assert.True(isVerifiedResponse);

        using (var scope = mockApi.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var verified = await db.Organisations.FirstAsync(x => x.Id == created.Id);
            Assert.Equal(OrganisationLicenceStatus.EmailVerified, verified.LicenceStatus);
        }
    }

    [Fact]
    public async Task CreateOrganisation_ReturnsValidationError_WhenRequiredFieldsMissing()
    {
        var org = new Organisation(); // all fields empty

        var response = await client.PostAsJsonAsync("/api/organisations", org);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("Name"));
        Assert.True(problem.Errors.ContainsKey("ContactName"));
        Assert.True(problem.Errors.ContainsKey("ContactEmail"));
    }

    [Fact]
    public async Task CreateOrganisation_ReturnsConflict_WhenNameAlreadyExists()
    {
        var org = NewOrganisation("UniqueOrg");
        var first = await client.PostAsJsonAsync("/api/organisations", org);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/organisations", org);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task GetOrganisation_ReturnsExistingOrganisation()
    {
        var org = NewOrganisation("GetOrg");
        var createResponse = await client.PostAsJsonAsync("/api/organisations", org);
        var created = await createResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/organisations/{created.Id}");
        getResponse.EnsureSuccessStatusCode();

        var fetched = await getResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(org.Name, fetched.Name);
    }

    [Fact]
    public async Task GetOrganisation_ReturnsNotFound_ForMissingId()
    {
        var response = await client.GetAsync($"/api/organisations/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListOrganisations_ReturnsAllOrganisations()
    {
        await client.PostAsJsonAsync("/api/organisations", NewOrganisation("OrgA"));
        await client.PostAsJsonAsync("/api/organisations", NewOrganisation("OrgB"));

        var response = await client.GetAsync("/api/organisations");
        response.EnsureSuccessStatusCode();

        var orgs = await response.Content.ReadFromJsonAsync<List<Organisation>>();
        Assert.NotNull(orgs);
        Assert.True(orgs.Count >= 2);
    }

    [Fact]
    public async Task UpdateOrganisation_ReturnsUpdatedOrganisation()
    {
        var org = NewOrganisation("UpdateOrg");
        var createResponse = await client.PostAsJsonAsync("/api/organisations", org);
        var created = await createResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(created);

        created.LicenceStatus = OrganisationLicenceStatus.Active;
        var putResponse = await client.PutAsJsonAsync($"/api/organisations/{created.Id}", created);
        putResponse.EnsureSuccessStatusCode();

        var updated = await putResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(updated);
        Assert.Equal(OrganisationLicenceStatus.Active, updated.LicenceStatus);
    }

    [Fact]
    public async Task UpdateOrganisation_ReturnsNotFound_ForMissingId()
    {
        var response = await client.PutAsJsonAsync($"/api/organisations/{Guid.NewGuid()}", NewOrganisation("Ghost"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrganisation_ReturnsNoContent()
    {
        var org = NewOrganisation("DeleteOrg");
        var createResponse = await client.PostAsJsonAsync("/api/organisations", org);
        var created = await createResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(created);

        var deleteResponse = await client.DeleteAsync($"/api/organisations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/organisations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteOrganisation_ReturnsConflict_WhenUsersLinked()
    {
        var org = NewOrganisation("LinkedOrg");
        var orgResponse = await client.PostAsJsonAsync("/api/organisations", org);
        var createdOrg = await orgResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(createdOrg);

        var user = new User { Email = "linked@example.com", OrganisationId = createdOrg.Id };
        var userResponse = await client.PostAsJsonAsync("/api/users", user);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/organisations/{createdOrg.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Auth / Health
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_Returns401_WhenUnauthenticated()
    {
        var anonClient = mockApi.CreateUnauthenticatedClient();
        var response = await anonClient.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RecordDownloadEvent_ReturnsOk_AndPersistsEvent()
    {
        const string email = "download-user@example.com";
        await mockApi.SeedUserAsync(new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            LicenceStatus = UserLicenceStatus.General,
            DateCreated = DateTime.UtcNow
        });

        var request = new DownloadEventRequest
        {
            Token = CreateDownloadAccessToken(email),
            DownloadType = "nextgen",
            Version = "2026.05.1234",
            Platform = "windows",
            DownloadUrl = "https://builds.apsim.info/example"
        };

        var response = await client.PostAsJsonAsync("/api/downloads/events", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = mockApi.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
        var audit = await db.DownloadAudits.FirstOrDefaultAsync();
        Assert.NotNull(audit);
        Assert.Equal(email, audit.UserEmail);
        Assert.Equal("nextgen", audit.DownloadType);
        Assert.Equal("2026.05.1234", audit.Version);
        Assert.Equal("windows", audit.Platform);
    }

    [Fact]
    public async Task RecordDownloadEvent_ReturnsBadRequest_WhenDownloadTypeInvalid()
    {
        var request = new DownloadEventRequest
        {
            Token = CreateDownloadAccessToken("invalid-type@example.com"),
            DownloadType = "legacy",
            Version = "1.0.0"
        };

        var response = await client.PostAsJsonAsync("/api/downloads/events", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListDownloadEvents_ReturnsFilteredEvents_ForAuthenticatedCaller()
    {
        var oldEventTime = DateTime.UtcNow.AddDays(-2);
        var newEventTime = DateTime.UtcNow.AddMinutes(-30);

        await mockApi.SeedDownloadAuditAsync(new DownloadAuditEntity
        {
            Id = Guid.NewGuid(),
            DownloadedAtUtc = oldEventTime,
            UserEmail = "classic-user@example.com",
            DownloadType = "classic",
            Version = "Revision 123",
            Platform = "windows"
        });

        await mockApi.SeedDownloadAuditAsync(new DownloadAuditEntity
        {
            Id = Guid.NewGuid(),
            DownloadedAtUtc = newEventTime,
            UserEmail = "nextgen-user@example.com",
            DownloadType = "nextgen",
            Version = "2026.05.999",
            Platform = "linux-debian"
        });

        var filterFrom = DateTime.UtcNow.AddHours(-1).ToString("O");
        var response = await client.GetAsync($"/api/downloads/events?downloadType=nextgen&fromUtc={Uri.EscapeDataString(filterFrom)}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<DownloadAuditListResponse>();
        Assert.NotNull(payload);
        Assert.Single(payload.Items);
        Assert.Equal("nextgen", payload.Items[0].DownloadType);
        Assert.Equal("nextgen-user@example.com", payload.Items[0].UserEmail);
    }

    [Fact]
    public async Task ListDownloadEvents_Returns401_WhenUnauthenticated()
    {
        var anonClient = mockApi.CreateUnauthenticatedClient();
        var response = await anonClient.GetAsync("/api/downloads/events");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListDownloadEvents_ReturnsBadRequest_WhenDownloadTypeInvalid()
    {
        var response = await client.GetAsync("/api/downloads/events?downloadType=legacy");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ExportDownloadEventsCsv_ReturnsFilteredCsv_ForAuthenticatedCaller()
    {
        await mockApi.SeedDownloadAuditAsync(new DownloadAuditEntity
        {
            Id = Guid.NewGuid(),
            DownloadedAtUtc = DateTime.UtcNow.AddHours(-4),
            UserEmail = "classic-user@example.com",
            DownloadType = "classic",
            Version = "Revision 111",
            Platform = "windows",
            DownloadUrl = "https://builds.apsim.info/old/111"
        });

        await mockApi.SeedDownloadAuditAsync(new DownloadAuditEntity
        {
            Id = Guid.NewGuid(),
            DownloadedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            UserEmail = "nextgen-user@example.com",
            DownloadType = "nextgen",
            Version = "2026.05.2000",
            Platform = "linux-debian",
            DownloadUrl = "https://builds.apsim.info/next/2000"
        });

        var response = await client.GetAsync("/api/downloads/events/export?downloadType=nextgen");
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        var lines = csv.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 2);
        Assert.Equal("DownloadedAtUtc,UserEmail,UserId,DownloadType,Version,Platform,DownloadUrl", lines[0]);
        Assert.Contains("nextgen-user@example.com", lines[1], StringComparison.Ordinal);
        Assert.DoesNotContain("classic-user@example.com", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExportDownloadEventsCsv_Returns401_WhenUnauthenticated()
    {
        var anonClient = mockApi.CreateUnauthenticatedClient();
        var response = await anonClient.GetAsync("/api/downloads/events/export");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExportDownloadEventsCsv_ReturnsBadRequest_WhenDownloadTypeInvalid()
    {
        var response = await client.GetAsync("/api/downloads/events/export?downloadType=legacy");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string CreateDownloadAccessToken(string email)
    {
        const string issuer = "registration-tests";
        const string audience = "registration-tests";
        const string signingKey = "registration-tests-signing-key-1234567890";

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("purpose", "download-access"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            notBefore: now,
            expires: now.AddMinutes(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class DownloadAuditListResponse
    {
        public int Total { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public List<DownloadAuditResponse> Items { get; set; } = new();
    }
}
