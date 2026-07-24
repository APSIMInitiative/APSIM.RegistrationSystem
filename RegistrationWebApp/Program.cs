using BlazoredGoogleCaptcha.Services;
using dotenv.net;
using Microsoft.AspNetCore.HttpOverrides;
using RegistrationWebApp.Components;
using RegistrationWebApp.Components.Utilities;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebApiUtility>();
builder.Services.AddScoped<DownloadAccessState>();
builder.Services.AddHttpClient<APSIMBuildsAPIUtility>();
builder.Services.AddSingleton<CaptchaService>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Force service construction on startup so WebApiUtility configuration is initialized.
_ = app.Services.GetRequiredService<WebApiUtility>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
