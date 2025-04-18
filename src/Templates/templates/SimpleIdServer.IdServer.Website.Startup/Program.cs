using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
var dataProtectionPath = builder.Configuration["dataProtectionPath"]?.ToString();
var isRealmEnabled = bool.Parse(builder.Configuration["IsRealmEnabled"]);
var cookieName = CookieAuthenticationDefaults.CookiePrefix + Uri.EscapeDataString("AdminWebsite");
builder.Services.AddSIDWebsite(o =>
{
    o.IdServerBaseUrl = builder.Configuration["IdServerBaseUrl"];
    o.SCIMUrl = builder.Configuration["ScimBaseUrl"];
    o.IsReamEnabled = isRealmEnabled;
}, (c) =>
{
    if(!string.IsNullOrWhiteSpace(dataProtectionPath))
    {
        c.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
    }
});
bool forceHttps = false;
var forceHttpsStr = builder.Configuration["forceHttps"];
if (!string.IsNullOrWhiteSpace(forceHttpsStr) && bool.TryParse(forceHttpsStr, out bool r))
    forceHttps = r;

builder.Services.AddDefaultSecurity(builder.Configuration, isRealmEnabled, cookieName);
builder.Services.AddLocalization();

var app = builder.Build();

if (forceHttps)
    app.SetHttpsScheme();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Services.AddSIDWebsite();
app.UseStaticFiles();
app.UseSidWebsite();
app.UseRequestLocalization(e =>
{
    e.SetDefaultCulture("en");
    e.AddSupportedCultures("en");
    e.AddSupportedUICultures("en");
});
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();
app.Run();