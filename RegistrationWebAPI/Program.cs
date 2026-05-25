using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RegistrationShared.Enums;
using RegistrationShared.Models;
using RegistrationWebAPI.Data;
using RegistrationWebAPI.Models;
using RegistrationWebAPI.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

// Load environment variables from .env file.
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration (mapped from Env__Var__Name to Section:Key).
builder.Configuration
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Auth:Password"] = Environment.GetEnvironmentVariable("Auth__Password"),
        ["Auth:Username"] = Environment.GetEnvironmentVariable("Auth__Username"),
        ["Jwt:Issuer"] = Environment.GetEnvironmentVariable("Jwt__Issuer"),
        ["Jwt:Audience"] = Environment.GetEnvironmentVariable("Jwt__Audience"),
        ["Jwt:SigningKey"] = Environment.GetEnvironmentVariable("Jwt__SigningKey"),
        ["Jwt:TokenExpiryMinutes"] = Environment.GetEnvironmentVariable("Jwt__TokenExpiryMinutes"),
        ["Verification:BaseUrl"] = Environment.GetEnvironmentVariable("Verification__BaseUrl"),
        ["Download:BaseUrl"] = Environment.GetEnvironmentVariable("Download__BaseUrl"),
        ["Download:TokenLifetimeHours"] = Environment.GetEnvironmentVariable("Download__TokenLifetimeHours"),
        ["Branding:LogoUrl"] = Environment.GetEnvironmentVariable("Branding__LogoUrl")
    });

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
var jwtExpiryMinutes = int.TryParse(builder.Configuration["Jwt:TokenExpiryMinutes"], out var expiryMinutes) ? expiryMinutes : 60;
var verificationTokenLifetimeHours = int.TryParse(builder.Configuration["Verification:TokenLifetimeHours"], out var tokenLifetimeHours) ? tokenLifetimeHours : 24;
var downloadTokenLifetimeHours = int.TryParse(builder.Configuration["Download:TokenLifetimeHours"], out var configuredDownloadTokenLifetimeHours)
    ? configuredDownloadTokenLifetimeHours
    : 48;
if (downloadTokenLifetimeHours <= 0)
{
    downloadTokenLifetimeHours = 48;
}

var downloadBaseUrl = builder.Configuration["Download:BaseUrl"] ?? builder.Configuration["Verification:BaseUrl"];
const string downloadAccessPurposeClaim = "download-access";

static string ResolveTemplateLogoUrl(string? configuredLogoUrl, string? baseUrl)
{
    if (!string.IsNullOrWhiteSpace(configuredLogoUrl))
    {
        return configuredLogoUrl;
    }

    return "https://www.apsim.info/wp-content/uploads/2026/05/APSIM_transparent-154x100-1.png";
}

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "APSIM Registration API",
        Description = "API for managing users and organisations in the APSIM Registration System."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
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
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

var verificationPagePath = Path.Combine(app.Environment.ContentRootPath, "verification.html");
if (!File.Exists(verificationPagePath))
{
    throw new InvalidOperationException($"Verification page was not found at '{verificationPagePath}'.");
}

var templateLogoUrl = ResolveTemplateLogoUrl(
    builder.Configuration["Branding:LogoUrl"],
    builder.Configuration["Verification:BaseUrl"] ?? downloadBaseUrl);

/// Load the verification page HTML and replace the placeholder with the configured base URL for verification links.
var verificationPageHtml = File.ReadAllText(verificationPagePath)
    .Replace("{{VerificationBaseUrl}}", builder.Configuration["Verification:BaseUrl"])
    .Replace("{{LogoUrl}}", templateLogoUrl);

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
    db.Database.EnsureCreated();
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

MailUtility? mailUtility = null;
var smtpApiKey = Environment.GetEnvironmentVariable("Smtp__ApiKey");
if (!string.IsNullOrWhiteSpace(smtpApiKey))
{
    mailUtility = new MailUtility(smtpApiKey);
}

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
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
        ExpiresAtUtc = expiresAt
    });
})
    .AllowAnonymous()
    .WithName("CreateAuthToken")
    .WithTags("Authentication")
    .Produces<AuthTokenResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("GetHealth")
    .WithTags("Health")
    .Produces(StatusCodes.Status200OK);

app.MapGet("/", () => Results.Ok(new
{
    service = "APSIM Registration API",
    status = "ok",
    health = "/health",
    swagger = "/swagger"
}))
    .WithName("GetRoot")
    .WithTags("Health")
    .Produces(StatusCodes.Status200OK);

app.MapGet("/api/downloads/link", async (string email, RegistrationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(downloadBaseUrl))
    {
        return Results.Problem("Download base URL is not configured.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest("Email is required.");
    }

    string normalizedEmail;
    try
    {
        normalizedEmail = new MailAddress(email.Trim()).Address;
    }
    catch (FormatException)
    {
        return Results.BadRequest("Email is not valid.");
    }

    var user = await db.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Email == normalizedEmail);

    if (user is null || user.LicenceStatus is UserLicenceStatus.None or UserLicenceStatus.Pending)
    {
        return Results.NotFound("No eligible registration was found for that email address.");
    }

    var now = DateTime.UtcNow;
    var expiresAt = now.AddHours(downloadTokenLifetimeHours);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, normalizedEmail),
        new Claim(JwtRegisteredClaimNames.Email, normalizedEmail),
        new Claim("purpose", downloadAccessPurposeClaim),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
    var downloadPageUrl = new Uri(new Uri(downloadBaseUrl), "download").ToString();
        var downloadLink = $"{downloadPageUrl}?token={Uri.EscapeDataString(tokenValue)}";
    
        if (mailUtility is not null)
        {
            await mailUtility.SendDownloadLinkEmailAsync(normalizedEmail, downloadLink);
            return Results.Ok(new { message = "Download link has been sent to your email address. It expires in 48 hours." });
        }
        else
        {
            return Results.Problem("Email service is not configured.", statusCode: StatusCodes.Status500InternalServerError);
        }
})
    .AllowAnonymous()
    .WithName("CreateDownloadAccessLink")
    .WithTags("Downloads")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapGet("/api/downloads/validate", (string token) =>
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.BadRequest("Download token is required.");
    }

    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    try
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
        var jwtToken = validatedToken as JwtSecurityToken;

        var purpose = principal.Claims.FirstOrDefault(c => c.Type == "purpose")?.Value;
        var email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
            ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (!string.Equals(purpose, downloadAccessPurposeClaim, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(email) || jwtToken is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            isValid = true,
            email,
            expiresAtUtc = jwtToken.ValidTo
        });
    }
    catch (SecurityTokenExpiredException)
    {
        return Results.BadRequest("Download token has expired.");
    }
    catch (SecurityTokenException)
    {
        return Results.Unauthorized();
    }
})
    .AllowAnonymous()
    .WithName("ValidateDownloadAccessToken")
    .WithTags("Downloads")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized);

var users = app.MapGroup("/api/users")
    .WithTags("Users")
    .RequireAuthorization();

users.MapGet("/", async (RegistrationDbContext db) =>
{
    if (!await db.Users.AnyAsync())
    {
        return Results.Ok(Array.Empty<User>());
    }

    var entities = await db.Users
        .AsNoTracking()
        .OrderBy(x => x.Email)
        .ToListAsync();

    return Results.Ok(entities.Select(ToUserModel));
})
    .WithName("ListUsers")
    .Produces<List<User>>(StatusCodes.Status200OK);

users.MapGet("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
    var entity = await db.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id);

    return entity is null ? Results.NotFound() : Results.Ok(ToUserModel(entity));
})
    .WithName("GetUserById")
    .Produces<User>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

users.MapPost("/", async (User user, RegistrationDbContext db, HttpContext http) =>
{
    var validationError = await ValidateUserAsync(user, db);
    if (validationError is not null)
    {
        return validationError;
    }

    var entity = ToUserEntity(user);
    if (entity.Id == Guid.Empty)
    {
        entity.Id = Guid.NewGuid();
    }

    if (entity.DateCreated == default)
    {
        entity.DateCreated = DateTime.UtcNow;
    }

    entity.LicenceStatus = UserLicenceStatus.Pending;
    entity.EmailVerificationToken = Guid.NewGuid().ToString("N");
    entity.EmailVerificationTokenExpiryUtc = DateTime.UtcNow.AddHours(verificationTokenLifetimeHours);

    db.Users.Add(entity);
    await db.SaveChangesAsync();

    if (mailUtility is not null)
    {
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var verificationLink = $"{baseUrl}/api/users/verify?token={Uri.EscapeDataString(entity.EmailVerificationToken)}";
        await mailUtility.SendVerificationEmailAsync(entity.Email, verificationLink);
    }

    return Results.Created($"/api/users/{entity.Id}", ToUserModel(entity));
})
    .WithName("CreateUser")
    .Produces<User>(StatusCodes.Status201Created)
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status409Conflict);

users.MapPut("/{id:guid}", async (Guid id, User user, RegistrationDbContext db) =>
{
    var entity = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null)
    {
        return Results.NotFound();
    }

    var validationError = await ValidateUserAsync(user, db, id);
    if (validationError is not null)
    {
        return validationError;
    }

    entity.Email = user.Email.Trim();
    entity.LicenceStatus = user.LicenceStatus;
    entity.OrganisationId = user.OrganisationId;

    await db.SaveChangesAsync();

    return Results.Ok(ToUserModel(entity));
})
    .WithName("UpdateUser")
    .Produces<User>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status409Conflict);

users.MapDelete("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
    var entity = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null)
    {
        return Results.NotFound();
    }

    db.Users.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .WithName("DeleteUser")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

users.MapGet("/verify", async (string token, RegistrationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.BadRequest("Verification token is required.");
    }

    var entity = await db.Users.FirstOrDefaultAsync(x => x.EmailVerificationToken == token);
    if (entity is null)
    {
        return Results.NotFound("Invalid verification token.");
    }

    if (entity.EmailVerificationTokenExpiryUtc is null || entity.EmailVerificationTokenExpiryUtc < DateTime.UtcNow)
    {
        return Results.BadRequest("Verification token has expired.");
    }

    entity.LicenceStatus = UserLicenceStatus.General;
    entity.EmailVerificationToken = null;
    entity.EmailVerificationTokenExpiryUtc = null;
    await db.SaveChangesAsync();

    return Results.Content(verificationPageHtml, "text/html");
})
    .AllowAnonymous()
    .WithName("VerifyUserEmail")
    .Produces(StatusCodes.Status200OK, contentType: "text/html")
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

var organisations = app.MapGroup("/api/organisations")
    .WithTags("Organisations")
    .RequireAuthorization();

organisations.MapGet("/", async (RegistrationDbContext db) =>
{
    var entities = await db.Organisations
        .AsNoTracking()
        .Include(x => x.Users)
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(entities.Select(ToOrganisationModel));
})
    .WithName("ListOrganisations")
    .Produces<List<Organisation>>(StatusCodes.Status200OK);

organisations.MapGet("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
    var entity = await db.Organisations
        .AsNoTracking()
        .Include(x => x.Users)
        .FirstOrDefaultAsync(x => x.Id == id);

    return entity is null ? Results.NotFound() : Results.Ok(ToOrganisationModel(entity));
})
    .WithName("GetOrganisationById")
    .Produces<Organisation>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

organisations.MapPost("/", async (Organisation organisation, RegistrationDbContext db, HttpContext http) =>
{
    var validationError = await ValidateOrganisationAsync(organisation, db);
    if (validationError is not null)
    {
        return validationError;
    }

    var entity = ToOrganisationEntity(organisation);
    if (entity.Id == Guid.Empty)
    {
        entity.Id = Guid.NewGuid();
    }

    if (entity.DateCreated == default)
    {
        entity.DateCreated = DateTime.UtcNow;
    }

    entity.LicenceStatus = OrganisationLicenceStatus.Pending;
    entity.EmailVerificationToken = Guid.NewGuid().ToString("N");
    entity.EmailVerificationTokenExpiryUtc = DateTime.UtcNow.AddHours(verificationTokenLifetimeHours);

    db.Organisations.Add(entity);
    await db.SaveChangesAsync();

    if (mailUtility is not null)
    {
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var verificationLink = $"{baseUrl}/api/organisations/verify?token={Uri.EscapeDataString(entity.EmailVerificationToken)}";
        await mailUtility.SendVerificationEmailAsync(entity.ContactEmail, verificationLink);
    }

    return Results.Created($"/api/organisations/{entity.Id}", ToOrganisationModel(entity));
})
    .WithName("CreateOrganisation")
    .Produces<Organisation>(StatusCodes.Status201Created)
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status409Conflict);

organisations.MapPut("/{id:guid}", async (Guid id, Organisation organisation, RegistrationDbContext db) =>
{
    var entity = await db.Organisations
        .Include(x => x.Users)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (entity is null)
    {
        return Results.NotFound();
    }

    var validationError = await ValidateOrganisationAsync(organisation, db, id);
    if (validationError is not null)
    {
        return validationError;
    }

    entity.Name = organisation.Name.Trim();
    entity.Emails = organisation.Emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    entity.LicenceStatus = organisation.LicenceStatus;
    entity.ContactName = organisation.ContactName.Trim();
    entity.ContactEmail = organisation.ContactEmail.Trim();
    entity.ContactPhone = organisation.ContactPhone.Trim();
    entity.ContactAddress = organisation.ContactAddress.Trim();
    entity.LicencePathway = organisation.LicencePathway;
    entity.AnnualTurnover = organisation.AnnualTurnover;

    await db.SaveChangesAsync();

    return Results.Ok(ToOrganisationModel(entity));
})
    .WithName("UpdateOrganisation")
    .Produces<Organisation>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status409Conflict);

organisations.MapDelete("/{id:guid}", async (Guid id, RegistrationDbContext db) =>
{
    var entity = await db.Organisations.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null)
    {
        return Results.NotFound();
    }

    var hasUsers = await db.Users.AnyAsync(x => x.OrganisationId == id);
    if (hasUsers)
    {
        return Results.Conflict("Cannot delete organisation while users are linked to it.");
    }

    db.Organisations.Remove(entity);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .WithName("DeleteOrganisation")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict);

organisations.MapGet("/verify", async (string token, RegistrationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.BadRequest("Verification token is required.");
    }

    var entity = await db.Organisations.FirstOrDefaultAsync(x => x.EmailVerificationToken == token);
    if (entity is null)
    {
        return Results.NotFound("Invalid verification token.");
    }

    if (entity.EmailVerificationTokenExpiryUtc is null || entity.EmailVerificationTokenExpiryUtc < DateTime.UtcNow)
    {
        return Results.BadRequest("Verification token has expired.");
    }

    entity.LicenceStatus = OrganisationLicenceStatus.Active;
    entity.EmailVerificationToken = null;
    entity.EmailVerificationTokenExpiryUtc = null;
    await db.SaveChangesAsync();

    return Results.Content(verificationPageHtml, "text/html");
})
    .AllowAnonymous()
    .WithName("VerifyOrganisationEmail")
    .Produces(StatusCodes.Status200OK, contentType: "text/html")
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

app.Run();

static User ToUserModel(UserEntity entity) =>
    new()
    {
        Id = entity.Id,
        Email = entity.Email,
        DateCreated = entity.DateCreated,
        LicenceStatus = entity.LicenceStatus,
        OrganisationId = entity.OrganisationId
    };

static UserEntity ToUserEntity(User model) =>
    new()
    {
        Id = model.Id,
        Email = model.Email.Trim(),
        DateCreated = model.DateCreated,
        LicenceStatus = model.LicenceStatus,
        OrganisationId = model.OrganisationId
    };

static Organisation ToOrganisationModel(OrganisationEntity entity) =>
    new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Emails = entity.Emails,
        LicenceStatus = entity.LicenceStatus,
        ContactName = entity.ContactName,
        ContactEmail = entity.ContactEmail,
        ContactPhone = entity.ContactPhone,
        ContactAddress = entity.ContactAddress,
        LicencePathway = entity.LicencePathway,
        AnnualTurnover = entity.AnnualTurnover,
        DateCreated = entity.DateCreated,
        Users = entity.Users.Select(ToUserModel).ToList()
    };

static OrganisationEntity ToOrganisationEntity(Organisation model) =>
    new()
    {
        Id = model.Id,
        Name = model.Name.Trim(),
        Emails = model.Emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        LicenceStatus = model.LicenceStatus,
        ContactName = model.ContactName.Trim(),
        ContactEmail = model.ContactEmail.Trim(),
        ContactPhone = model.ContactPhone.Trim(),
        ContactAddress = model.ContactAddress.Trim(),
        LicencePathway = model.LicencePathway,
        AnnualTurnover = model.AnnualTurnover,
        DateCreated = model.DateCreated,
        Users = model.Users.Select(ToUserEntity).ToList()
    };

static async Task<IResult?> ValidateUserAsync(User user, RegistrationDbContext db, Guid? currentUserId = null)
{
    if (string.IsNullOrWhiteSpace(user.Email))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["email"] = ["Email is required."]
        });
    }

    try
    {
        _ = new MailAddress(user.Email);
    }
    catch (FormatException)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["email"] = ["Email is not valid."]
        });
    }

    var normalizedEmail = user.Email.Trim();

    var duplicateEmail = await db.Users.AnyAsync(x =>
        x.Email == normalizedEmail &&
        (!currentUserId.HasValue || x.Id != currentUserId.Value));

    if (duplicateEmail)
    {
        return Results.Conflict("A user with the same email already exists.");
    }

    if (user.OrganisationId != Guid.Empty)
    {
        var organisationExists = await db.Organisations.AnyAsync(x => x.Id == user.OrganisationId);
        if (!organisationExists)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["organisationId"] = ["Organisation does not exist."]
            });
        }
    }

    return null;
}

static async Task<IResult?> ValidateOrganisationAsync(Organisation organisation, RegistrationDbContext db, Guid? currentOrganisationId = null)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(organisation.Name))
    {
        errors["name"] = ["Name is required."];
    }

    if (string.IsNullOrWhiteSpace(organisation.ContactName))
    {
        errors["contactName"] = ["ContactName is required."];
    }

    if (string.IsNullOrWhiteSpace(organisation.ContactEmail))
    {
        errors["contactEmail"] = ["ContactEmail is required."];
    }
    else
    {
        try
        {
            _ = new MailAddress(organisation.ContactEmail);
        }
        catch (FormatException)
        {
            errors["contactEmail"] = ["ContactEmail is not valid."];
        }
    }

    if (string.IsNullOrWhiteSpace(organisation.ContactPhone))
    {
        errors["contactPhone"] = ["ContactPhone is required."];
    }

    if (string.IsNullOrWhiteSpace(organisation.ContactAddress))
    {
        errors["contactAddress"] = ["ContactAddress is required."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var normalizedName = organisation.Name.Trim();

    var duplicateName = await db.Organisations.AnyAsync(x =>
        x.Name == normalizedName &&
        (!currentOrganisationId.HasValue || x.Id != currentOrganisationId.Value));

    if (duplicateName)
    {
        return Results.Conflict("An organisation with the same name already exists.");
    }

    return null;
}

/// <summary>Marker type used by WebApplicationFactory to locate the RegistrationWebAPI entry point.</summary>
public class RegistrationWebApiMarker { }
