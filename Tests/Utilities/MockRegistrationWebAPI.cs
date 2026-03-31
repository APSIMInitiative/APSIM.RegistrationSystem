using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RegistrationWebAPI;
using RegistrationWebAPI.Data;
using RegistrationWebAPI.Models;

namespace Tests.Utilities;

public sealed class MockRegistrationWebAPI : WebApplicationFactory<RegistrationWebApiMarker>
{
	public const string TestUsername = "test-user";
	public const string TestPassword = "test-password";

	private readonly SqliteConnection connection = new("Data Source=:memory:");
	private readonly Dictionary<string, string?> originalEnvironmentVariables = new();

	public MockRegistrationWebAPI()
	{
		SetEnvironmentVariable("Auth__Username", TestUsername);
		SetEnvironmentVariable("Auth__Password", TestPassword);
		SetEnvironmentVariable("Jwt__Issuer", "registration-tests");
		SetEnvironmentVariable("Jwt__Audience", "registration-tests");
		SetEnvironmentVariable("Jwt__SigningKey", "registration-tests-signing-key-1234567890");
		SetEnvironmentVariable("Jwt__TokenExpiryMinutes", "60");
		SetEnvironmentVariable("ConnectionStrings__RegistrationDb", "Data Source=registration-tests");
		connection.Open();
	}

	public HttpClient CreateUnauthenticatedClient()
	{
		return CreateClient(new WebApplicationFactoryClientOptions
		{
			BaseAddress = new Uri("https://localhost"),
		});
	}

	public async Task<HttpClient> CreateAuthenticatedClientAsync()
	{
		var client = CreateUnauthenticatedClient();
		var token = await GetAuthTokenAsync(client);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		return client;
	}

	public async Task<string> GetAuthTokenAsync(HttpClient? client = null)
	{
		var authClient = client ?? CreateUnauthenticatedClient();
		var ownsClient = client is null;

		try
		{
			var response = await authClient.PostAsJsonAsync("/api/auth/token", new AuthTokenRequest
			{
				Username = TestUsername,
				Password = TestPassword,
			});

			response.EnsureSuccessStatusCode();

			var tokenResponse = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
			if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
			{
				throw new InvalidOperationException("Authentication token response was empty.");
			}

			return tokenResponse.AccessToken;
		}
		finally
		{
			if (ownsClient)
			{
				authClient.Dispose();
			}
		}
	}

	public async Task SeedRegistrationAsync(RegistrationEntity entity)
	{
		using var scope = Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();

		dbContext.Registrations.Add(entity);
		await dbContext.SaveChangesAsync();
	}

	public async Task ResetDatabaseAsync()
	{
		using var scope = Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();

		await dbContext.Database.EnsureDeletedAsync();
		await dbContext.Database.EnsureCreatedAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");

		builder.ConfigureAppConfiguration((_, configBuilder) =>
		{
			configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Auth:Username"] = TestUsername,
				["Auth:Password"] = TestPassword,
				["Jwt:Issuer"] = "registration-tests",
				["Jwt:Audience"] = "registration-tests",
				["Jwt:SigningKey"] = "registration-tests-signing-key-1234567890",
				["Jwt:TokenExpiryMinutes"] = "60",
				["ConnectionStrings:RegistrationDb"] = "Data Source=registration-tests",
			});
		});

		builder.ConfigureServices(services =>
		{
			services.RemoveAll(typeof(DbContextOptions<RegistrationDbContext>));

			services.AddDbContext<RegistrationDbContext>(options =>
			{
				options.UseSqlite(connection);
			});

			using var scope = services.BuildServiceProvider().CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
			dbContext.Database.EnsureCreated();
		});
	}

	public override async ValueTask DisposeAsync()
	{
		foreach (var entry in originalEnvironmentVariables)
		{
			Environment.SetEnvironmentVariable(entry.Key, entry.Value);
		}

		await base.DisposeAsync();
		await connection.DisposeAsync();
	}

	private void SetEnvironmentVariable(string key, string value)
	{
		if (!originalEnvironmentVariables.ContainsKey(key))
		{
			originalEnvironmentVariables[key] = Environment.GetEnvironmentVariable(key);
		}

		Environment.SetEnvironmentVariable(key, value);
	}
}

public class RegistrationWebApiMarker
{
}