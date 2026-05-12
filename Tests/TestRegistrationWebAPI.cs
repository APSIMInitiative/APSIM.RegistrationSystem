using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RegistrationShared.Enums;
using RegistrationShared.Models;
using RegistrationWebAPI.Data;
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
        Assert.Equal("text/html", verifyResponse.Content.Headers.ContentType?.MediaType);
        var html = await verifyResponse.Content.ReadAsStringAsync();
        Assert.Contains("Email Verified", html, StringComparison.Ordinal);

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
    }

    [Fact]
    public async Task VerifyOrganisationEmail_UpdatesLicenceStatus()
    {
        var createResponse = await client.PostAsJsonAsync("/api/organisations", NewOrganisation("VerifyOrg"));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<Organisation>();
        Assert.NotNull(created);

        string token;
        using (var scope = mockApi.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var entity = await db.Organisations.FirstAsync(x => x.Id == created.Id);
            Assert.NotNull(entity.EmailVerificationToken);
            token = entity.EmailVerificationToken!;
        }

        var verifyResponse = await client.GetAsync($"/api/organisations/verify?token={token}");
        verifyResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/html", verifyResponse.Content.Headers.ContentType?.MediaType);
        var html = await verifyResponse.Content.ReadAsStringAsync();
        Assert.Contains("Email Verified", html, StringComparison.Ordinal);

        using (var scope = mockApi.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var verified = await db.Organisations.FirstAsync(x => x.Id == created.Id);
            Assert.Equal(OrganisationLicenceStatus.Active, verified.LicenceStatus);
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
        Assert.True(problem.Errors.ContainsKey("name"));
        Assert.True(problem.Errors.ContainsKey("contactName"));
        Assert.True(problem.Errors.ContainsKey("contactEmail"));
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
}
