using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RegistrationWebAPI.Models;
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

    [Fact]
    public async Task CreateRegistration_ReturnsCreatedRegistration()
    {
        var request = new RegistrationUpsertRequest
        {
            RegistrationType = RegistrationType.GeneralUse,
            ContactEmail = "test@example.com",
            ContactName = "Test User",
            ApplicationDate = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
        };

        var response = await client.PostAsJsonAsync("/api/registrations", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var registration = await response.Content.ReadFromJsonAsync<RegistrationResponse>();

        Assert.NotNull(registration);
        Assert.Equal(request.ContactEmail, registration.ContactEmail);
        Assert.Equal(request.ContactName, registration.ContactName);
    }

    [Fact]
    public async Task CreateRegistration_ReturnsValidationErrors()
    {
        var request = new RegistrationUpsertRequest
        {
            RegistrationType = RegistrationType.GeneralUse,
            ContactEmail = "invalid-email",
            ContactName = "",
            ApplicationDate = DateTime.UtcNow.AddDays(1),
        };

        var response = await client.PostAsJsonAsync("/api/registrations", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.Errors.ContainsKey(nameof(RegistrationUpsertRequest.ContactEmail)));
        Assert.True(problemDetails.Errors.ContainsKey(nameof(RegistrationUpsertRequest.ContactName)));
    }

    [Fact]
    public async Task GetRegistration_ReturnsExistingRegistration()
    {
        var createRequest = new RegistrationUpsertRequest
        {
            RegistrationType = RegistrationType.GeneralUse,
            ContactEmail = "test@example.com",
            ContactName = "Test User",
            ApplicationDate = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
        };

        var createResponse = await client.PostAsJsonAsync("/api/registrations", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createdRegistration = await createResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(createdRegistration);

        var getResponse = await client.GetAsync($"/api/registrations/{createdRegistration.Id}");
        getResponse.EnsureSuccessStatusCode();

        var registration = await getResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(registration);
        Assert.Equal(createdRegistration.Id, registration.Id);
        Assert.Equal(createRequest.ContactEmail, registration.ContactEmail);
        Assert.Equal(createRequest.ContactName, registration.ContactName);
    }

    [Fact]
    public async Task GetRegistration_ReturnsNotFoundForNonExistingRegistration()
    {
        var getResponse = await client.GetAsync("/api/registrations/non-existing-id");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetRegistrations_ReturnsListOfRegistrations()
    {
        var createRequest1 = new RegistrationUpsertRequest
        {
            RegistrationType = RegistrationType.GeneralUse,
            ContactEmail = "test1@example.com",
            ContactName = "Test User 1",
            ApplicationDate = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
        };

        var createRequest2 = new RegistrationUpsertRequest
        {
            RegistrationType = RegistrationType.GeneralUse,
            ContactEmail = "test2@example.com",
            ContactName = "Test User 2",
            ApplicationDate = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
        };

        await client.PostAsJsonAsync("/api/registrations", createRequest1);
        await client.PostAsJsonAsync("/api/registrations", createRequest2);

        var getResponse = await client.GetAsync("/api/registrations");
        getResponse.EnsureSuccessStatusCode();

        var registrations = await getResponse.Content.ReadFromJsonAsync<List<RegistrationResponse>>();
        Assert.NotNull(registrations);
        Assert.True(registrations.Count >= 2);
    }

    [Fact]
    public async Task DeleteRegistration_ReturnsNoContent()
    {
        var createRequest = new RegistrationUpsertRequest
        {
            RegistrationType = RegistrationType.GeneralUse,
            ContactEmail = "test@example.com",
            ContactName = "Test User",
            ApplicationDate = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
        };  

        var createResponse = await client.PostAsJsonAsync("/api/registrations", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdRegistration = await createResponse.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(createdRegistration);
        var deleteResponse = await client.DeleteAsync($"/api/registrations/{createdRegistration.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var getResponse = await client.GetAsync($"/api/registrations/{createdRegistration.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
