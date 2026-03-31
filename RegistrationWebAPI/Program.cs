using RegistrationShared.Enums;
using RegistrationWebAPI.Data;
using RegistrationWebAPI.Models;
using RegistrationWebAPI.Services;
using dotenv.net;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Load environment variables from .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration (mapped from Env__Var__Name to Section:Key)
builder.Configuration
	.AddInMemoryCollection(new Dictionary<string, string?>
	{
		["Auth:Password"] = Environment.GetEnvironmentVariable("Auth__Password"),
        ["Auth:Username"] = Environment.GetEnvironmentVariable("Auth__Username"),
		["Jwt:Issuer"] = Environment.GetEnvironmentVariable("Jwt__Issuer"),
		["Jwt:Audience"] = Environment.GetEnvironmentVariable("Jwt__Audience"),
		["Jwt:SigningKey"] = Environment.GetEnvironmentVariable("Jwt__SigningKey"),
		["Jwt:TokenExpiryMinutes"] = Environment.GetEnvironmentVariable("Jwt__TokenExpiryMinutes"),
	});

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
var jwtExpiryMinutes = int.TryParse(builder.Configuration["Jwt:TokenExpiryMinutes"], out var expiryMinutes) ? expiryMinutes : 60;

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "APSIM Registration API",
		Description = "API for managing registrations in the APSIM Registration System.",

	});

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter a valid JWT bearer token.",
	});

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer",
				},
			},
			Array.Empty<string>()
		}
	});
});
builder.Services.AddDbContext<RegistrationDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("RegistrationDb")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true,
			ValidIssuer = jwtIssuer,
			ValidAudience = jwtAudience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
			ClockSkew = TimeSpan.FromMinutes(1),
		};
	});
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "APSIM Registration API v1");
		options.DocumentTitle = "APSIM Registration API Documentation";
	});
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/token", (AuthTokenRequest request) =>
{
	var configuredUsername = builder.Configuration["Auth:Username"];
	var configuredPassword = builder.Configuration["Auth:Password"];

	if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
	{
		return Results.Problem("Auth credentials are not configured.", statusCode: StatusCodes.Status500InternalServerError);
	}

	if (!string.Equals(request.Username, configuredUsername, StringComparison.Ordinal) ||
		!string.Equals(request.Password, configuredPassword, StringComparison.Ordinal))
	{
		return Results.Unauthorized();
	}

	var now = DateTime.UtcNow;
	var expiresAt = now.AddMinutes(jwtExpiryMinutes);

	var claims = new[]
	{
		new Claim(JwtRegisteredClaimNames.Sub, configuredUsername),
		new Claim(JwtRegisteredClaimNames.UniqueName, configuredUsername),
		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
	};

	var signingCredentials = new SigningCredentials(
		new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
		SecurityAlgorithms.HmacSha256);

	var token = new JwtSecurityToken(
		issuer: jwtIssuer,
		audience: jwtAudience,
		claims: claims,
		notBefore: now,
		expires: expiresAt,
		signingCredentials: signingCredentials);

	var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

	return Results.Ok(new AuthTokenResponse
	{
		AccessToken = tokenValue,
		ExpiresAtUtc = expiresAt,
	});
})
	.AllowAnonymous()
	.WithName("CreateAuthToken")
	.WithTags("Authentication")
	.WithSummary("Create JWT token")
	.WithDescription("Authenticates the caller with configured credentials and returns a JWT token.")
	.Produces<AuthTokenResponse>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status401Unauthorized)
	.ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
	.WithName("GetHealth")
	.WithTags("Health")
    .WithSummary("Get API Health")
    .WithDescription("Returns a simple status message indicating that the API is running.")
	.Produces(StatusCodes.Status200OK);

var registrations = app.MapGroup("/api/registrations")
	.WithTags("Registrations")
    .RequireAuthorization();

registrations.MapGet("/", async (
	RegistrationDbContext db,
	RegistrationType? registrationType,
	LicenceStatus? licenceStatus,
	string? contactEmail) =>
{
	var query = db.Registrations.AsNoTracking().AsQueryable();

	if (registrationType.HasValue)
	{
		query = query.Where(x => x.RegistrationType == registrationType.Value);
	}

	if (licenceStatus.HasValue)
	{
		query = query.Where(x => x.LicenceStatus == licenceStatus.Value);
	}

	if (!string.IsNullOrWhiteSpace(contactEmail))
	{
		query = query.Where(x => x.ContactEmail == contactEmail);
	}

	var entities = await query
		.OrderByDescending(x => x.ApplicationDate)
		.ToListAsync();

	var data = entities.Select(RegistrationMapping.ToResponse).ToList();

	return Results.Ok(data);
})
	.WithName("ListRegistrations")
    .WithSummary("List Registrations")
    .WithDescription("Returns a list of registrations, optionally filtered by registration type, licence status, or contact email.")
	.Produces<List<RegistrationResponse>>(StatusCodes.Status200OK)
    .Produces<List<RegistrationErrorResponse>>(StatusCodes.Status500InternalServerError);

registrations.MapGet("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
	var registration = await db.Registrations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
	if (registration is null)
	{
		return Results.NotFound();
	}

	return Results.Ok(RegistrationMapping.ToResponse(registration));
})
	.WithName("GetRegistrationById")
    .WithSummary("Get Registration By Id")
    .WithDescription("Returns a registration by its unique identifier.")
	.Produces<RegistrationResponse>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status404NotFound);

// --- Add Registration Endpoint ---
registrations.MapPost("/", async (RegistrationUpsertRequest request, RegistrationDbContext db, ILogger<Program> logger) =>
{
	var errors = RegistrationValidation.Validate(request);
	if (errors.Count > 0)
	{
		return Results.ValidationProblem(errors);
	}

	try
	{
		var entity = RegistrationMapping.ToNewEntity(request);
		db.Registrations.Add(entity);
		await db.SaveChangesAsync();

		var response = RegistrationMapping.ToResponse(entity);
		return Results.Created($"/api/registrations/{entity.Id}", response);
	}
	catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqliteEx)
	{
		logger.LogError(ex, "Failed to create registration for {ContactEmail}. SQLiteErrorCode: {SQLiteErrorCode}", request.ContactEmail, sqliteEx.SqliteErrorCode);

		var errorResponse = new RegistrationErrorResponse
		{
			Message = "Registration could not be created because of a database constraint or data persistence error.",
			Errors = new Dictionary<string, string[]>
			{
				["database"] =
				[
					sqliteEx.Message,
					$"SQLiteErrorCode: {sqliteEx.SqliteErrorCode}"
				]
			}
		};

		return Results.Json(errorResponse, statusCode: StatusCodes.Status500InternalServerError);
	}
	catch (DbUpdateException ex)
	{
		logger.LogError(ex, "Failed to create registration for {ContactEmail}.", request.ContactEmail);

		var errorResponse = new RegistrationErrorResponse
		{
			Message = "Registration could not be created due to a data persistence error.",
			Errors = new Dictionary<string, string[]>
			{
				["database"] = [ex.InnerException?.Message ?? ex.Message]
			}
		};

		return Results.Json(errorResponse, statusCode: StatusCodes.Status500InternalServerError);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Unexpected failure while creating registration for {ContactEmail}.", request.ContactEmail);

		var errorResponse = new RegistrationErrorResponse
		{
			Message = "An unexpected error occurred while creating the registration.",
			Errors = new Dictionary<string, string[]>
			{
				["server"] = [ex.Message]
			}
		};

		return Results.Json(errorResponse, statusCode: StatusCodes.Status500InternalServerError);
	}
})
	.WithName("CreateRegistration")
    .WithDescription("Creates a new registration.")
    .WithSummary("Create Registration")
	.Produces<RegistrationResponse>(StatusCodes.Status201Created)
	.ProducesValidationProblem()
	.Produces<RegistrationErrorResponse>(StatusCodes.Status500InternalServerError);

registrations.MapPut("/{id:guid}", async (Guid id, RegistrationUpsertRequest request, RegistrationDbContext db) =>
{
	var errors = RegistrationValidation.Validate(request);
	if (errors.Count > 0)
	{
		return Results.ValidationProblem(errors);
	}

	var entity = await db.Registrations.FirstOrDefaultAsync(x => x.Id == id);
	if (entity is null)
	{
		return Results.NotFound();
	}

	RegistrationMapping.ApplyUpdate(entity, request);
	await db.SaveChangesAsync();

	return Results.Ok(RegistrationMapping.ToResponse(entity));
})
	.WithName("UpdateRegistration")
    .WithSummary("Update Registration")
    .WithDescription("Updates an existing registration by its unique identifier.")
	.Produces<RegistrationResponse>(StatusCodes.Status200OK)
	.Produces<RegistrationErrorResponse>(StatusCodes.Status500InternalServerError)
	.Produces(StatusCodes.Status404NotFound);

registrations.MapDelete("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
	var entity = await db.Registrations.FirstOrDefaultAsync(x => x.Id == id);
	if (entity is null)
	{
		return Results.NotFound();
	}

	db.Registrations.Remove(entity);
	await db.SaveChangesAsync();
	return Results.NoContent();
})
	.WithName("DeleteRegistration")
    .WithSummary("Delete Registration")
    .WithDescription("Deletes an existing registration by its unique identifier.")
	.Produces(StatusCodes.Status204NoContent)
	.Produces(StatusCodes.Status404NotFound);

app.Run();
