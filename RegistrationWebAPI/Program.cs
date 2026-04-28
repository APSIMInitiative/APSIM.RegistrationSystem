using RegistrationShared.Enums;
using RegistrationWebAPI.Data;
using RegistrationWebAPI.Models;
using RegistrationWebAPI.Services;
using dotenv.net;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RegistrationWebAPI.Utilities;
using Microsoft.OpenApi.Extensions;

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

builder.Services.AddSingleton(options =>
{
    string apiKey = builder.Configuration["Smtp:ApiKey"] ?? throw new InvalidOperationException("SendGrid API key is not configured.");
    return new MailUtility(apiKey);
});
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "APSIM Registration API",
		Description = "API for managing registrations in the APSIM Registration System.\n" +
		"To start use the Authentication endpoint with your credentials (from vault or .env) to retrieve a JWT token",
		
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

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
	db.Database.Migrate();
}

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
	.WithDescription("Authenticates the caller with configured credentials and returns a JWT token.\n"+
		"Change the username and password values below and use the token to authorize API requests above.\n" +
		"When the authorize padlock is 'locked' all subsequent endpoint requests will be authenticated.")
	.Produces<AuthTokenResponse>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status401Unauthorized)
	.ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
	.WithName("GetHealth")
	.WithTags("Health")
    .WithSummary("Get API Health")
    .WithDescription("Returns a simple status message indicating that the API is running.")
	.Produces(StatusCodes.Status200OK);

app.MapGet("/api/registrations/verify", async (string token, RegistrationDbContext db) =>
{
	if (string.IsNullOrWhiteSpace(token))
	{
		return Results.BadRequest("Verification token is required.");
	}

	var registration = await db.Registrations.FirstOrDefaultAsync(x => x.EmailVerificationToken == token);
	if (registration is null)
	{
		return Results.NotFound();
	}

	if (registration.LicenceStatus != LicenceStatus.AwaitingEmailVerification)
	{
		string message = $"Registration is not awaiting email verification. Current status: {registration.LicenceStatus.GetDisplayName()}";
		return Results.BadRequest(message);
	}

	if (registration.EmailVerificationSentAtUtc is null)
	{
		return Results.BadRequest("Verification link metadata is missing.");
	}

	if (DateTime.UtcNow > registration.EmailVerificationSentAtUtc.Value.AddHours(24))
	{
		return Results.BadRequest("Verification link has expired.");
	}

	registration.LicenceStatus = registration.RegistrationType == RegistrationType.SpecialUse
		? LicenceStatus.SpecialAwaitingReview
		: LicenceStatus.GeneralUse;
	registration.EmailVerificationToken = null;
	registration.EmailVerificationSentAtUtc = null;

	await db.SaveChangesAsync();

	return Results.Ok(new RegistrationResponse
	{
		Id = registration.Id,
		RegistrationType = registration.RegistrationType,
		ContactName = registration.ContactName,
		ContactEmail = registration.ContactEmail,
		ApplicationDate = registration.ApplicationDate,
		LicenceStatus = registration.LicenceStatus,
		OrganisationName = registration.OrganisationName,
		OrganisationAddress = registration.OrganisationAddress,
		OrganisationWebsite = registration.OrganisationWebsite,
		ContactPhone = registration.ContactPhone,
		LicencePathway = registration.LicencePathway,
		AnnualTurnover = registration.AnnualTurnover,
	});
})
	.AllowAnonymous()
	.WithName("VerifyRegistrationEmail")
	.WithTags("Registrations")
	.WithSummary("Verify registration email")
	.WithDescription("Verifies an email token and updates licence status within 24 hours of email send time.")
	.Produces<RegistrationResponse>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status400BadRequest)
	.Produces(StatusCodes.Status404NotFound);

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
registrations.MapPost("/", async (RegistrationUpsertRequest request, RegistrationDbContext db, ILogger<Program> logger, HttpContext httpContext) =>
{
	var errors = RegistrationValidation.Validate(request);
	if (errors.Count > 0)
	{
		return Results.ValidationProblem(errors);
	}

	try
	{
		var entity = RegistrationMapping.ToNewEntity(request);
		entity.EmailVerificationToken = Guid.NewGuid().ToString("N");
		entity.EmailVerificationSentAtUtc = DateTime.UtcNow;

		if (db.Registrations.Any(r => r.ContactEmail == entity.ContactEmail) is false)
		{
			db.Registrations.Add(entity);
			await db.SaveChangesAsync();
		}
		else return Results.Conflict("A registration with the same details already exists. You may already be registered.");

		var response = RegistrationMapping.ToResponse(entity);
		var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
		var verificationLink = $"{baseUrl}/api/registrations/verify?token={Uri.EscapeDataString(entity.EmailVerificationToken)}";

		// Send confirmation email after successful registration creation with a verification link.
		var mailUtility = app.Services.GetRequiredService<MailUtility>();
		await mailUtility.SendVerificationEmailAsync(entity.ContactEmail, verificationLink);

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


registrations.MapDelete("/", async ([FromBody] BulkDeleteRegistrationsRequest request, RegistrationDbContext db) =>
{
	if (request.Ids.Count == 0)
	{
		return Results.BadRequest("At least one registration id is required.");
	}

	var entities = await db.Registrations.Where(x => request.Ids.Contains(x.Id)).ToListAsync();
	if (entities.Count == 0)
	{
		return Results.NotFound();
	}

	db.Registrations.RemoveRange(entities);
	await db.SaveChangesAsync();
	return Results.NoContent();
})
	.WithName("BulkDeleteRegistrations")
	.WithSummary("Bulk Delete Registrations")
	.WithDescription("Deletes multiple registrations by their unique identifiers. Accepts a list of registration ids in the request body and deletes all matching registrations.")
	.Produces(StatusCodes.Status400BadRequest)
	.Produces(StatusCodes.Status204NoContent)
	.Produces(StatusCodes.Status404NotFound);

// --- Member Organisations Endpoints ---
// These endpoints allow management of member organisations, which allow staff
// to register on behalf of member organisations, and provide a way to group 
// registrations by organisation in the admin UI.

var memberOrganisations = app.MapGroup("/api/member-organisations")
	.WithTags("Member Organisations")
    .RequireAuthorization();

memberOrganisations.MapGet("/", async (RegistrationDbContext db) =>
{
	var entities = await db.MemberOrganisations
		.AsNoTracking()
		.OrderBy(x => x.OrganisationName)
		.ToListAsync();

	var data = entities.Select(MemberOrganisationMapping.ToResponse).ToList();

	return Results.Ok(data);
})
	.WithName("ListMemberOrganisations")
	.WithSummary("List Member Organisations")
	.WithDescription("Returns all member organisations.")
	.Produces<List<MemberOrganisationResponse>>(StatusCodes.Status200OK);

memberOrganisations.MapGet("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
	var entity = await db.MemberOrganisations
		.AsNoTracking()
		.FirstOrDefaultAsync(x => x.Id == id);

	if (entity is null)
	{
		return Results.NotFound();
	}

	return Results.Ok(MemberOrganisationMapping.ToResponse(entity));
})
	.WithName("GetMemberOrganisationById")
	.WithSummary("Get Member Organisation By Id")
	.WithDescription("Returns a member organisation by its unique identifier.")
	.Produces<MemberOrganisationResponse>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status404NotFound);


memberOrganisations.MapPost("/", async (MemberOrganisationUpsertRequest request, RegistrationDbContext db) =>
{
	var entity = MemberOrganisationMapping.ToNewEntity(request);
	if (db.MemberOrganisations.Any(mo => mo.OrganisationDomain == entity.OrganisationDomain))
		return Results.Conflict("A member organisation with the same domain already exists.");
	db.MemberOrganisations.Add(entity);
	await db.SaveChangesAsync();
	return Results.Created($"/api/member-organisations/{entity.Id}", MemberOrganisationMapping.ToResponse(entity));
})
	.WithName("CreateMemberOrganisation")
	.WithSummary("Create Member Organisation")
	.WithDescription("Creates a new member organisation.")
	.Produces<MemberOrganisationResponse>(StatusCodes.Status201Created)
	.ProducesValidationProblem()
	.Produces(StatusCodes.Status409Conflict);

memberOrganisations.MapPut("/{id:guid}", async (Guid id, MemberOrganisationUpsertRequest request, RegistrationDbContext db) =>
{
	var entity = await db.MemberOrganisations.FirstOrDefaultAsync(x => x.Id == id);
	if (entity is null)
	{
		return Results.NotFound();
	}

	MemberOrganisationMapping.ApplyUpdate(entity, request);
	await db.SaveChangesAsync();

	return Results.Ok(MemberOrganisationMapping.ToResponse(entity));
})
	.WithName("UpdateMemberOrganisation")
	.WithSummary("Update Member Organisation")
	.WithDescription("Updates an existing member organisation by its unique identifier.")
	.Produces<MemberOrganisationResponse>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status404NotFound);

memberOrganisations.MapDelete("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
	var entity = await db.MemberOrganisations.FirstOrDefaultAsync(x => x.Id == id);
	if (entity is null)
	{
		return Results.NotFound();
	}

	db.MemberOrganisations.Remove(entity);
	await db.SaveChangesAsync();

	return Results.NoContent();
})
	.WithName("DeleteMemberOrganisation")
	.WithSummary("Delete Member Organisation")
	.WithDescription("Deletes an existing member organisation by its unique identifier.")
	.Produces(StatusCodes.Status204NoContent)
	.Produces(StatusCodes.Status404NotFound);

app.Run();
