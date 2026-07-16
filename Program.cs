using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
using SpendiTrackWeb.Services.Email;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var philippineCulture = new CultureInfo("en-PH");
CultureInfo.DefaultThreadCurrentCulture = philippineCulture;
CultureInfo.DefaultThreadCurrentUICulture = philippineCulture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(philippineCulture);
    options.SupportedCultures = [philippineCulture];
    options.SupportedUICultures = [philippineCulture];
});

// Shared hosting sits behind a reverse proxy (HTTPS termination).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. On MonsterASP set env var " +
        "ConnectionStrings__DefaultConnection to your full MSSQL connection string.");

if (builder.Environment.IsProduction()
    && connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Production is still using LocalDB from appsettings.json. Set MonsterASP env var " +
        "ConnectionStrings__DefaultConnection and restart the site.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Replace Identity UI's NoOp / default adapters with real SMTP (or App_Data file dump).
builder.Services.AddTransient<IEmailSender, AppEmailSender>();
builder.Services.AddTransient<IEmailSender<IdentityUser>, IdentityEmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<SpendiTrackWeb.Services.BudgetCalculator>();
builder.Services.AddScoped<SpendiTrackWeb.Services.MonthlyBudgetService>();
builder.Services.AddScoped<SpendiTrackWeb.Services.TrackerAccessService>();

var app = builder.Build();

app.UseForwardedHeaders();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation(
    "Starting SpendiTrack. Environment={Environment}, DataSource={DataSource}",
    app.Environment.EnvironmentName,
    DescribeConnectionTarget(connectionString));

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    startupLogger.LogInformation("Applying database migrations...");
    db.Database.Migrate();
    startupLogger.LogInformation("Database migrations complete.");

    // Accounts created before email confirmation was required used email as username.
    // Confirm those so existing users are not locked out.
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var legacyUsers = await userManager.Users
        .Where(u => !u.EmailConfirmed && u.UserName != null && u.Email != null && u.UserName == u.Email)
        .ToListAsync();

    foreach (var legacy in legacyUsers)
    {
        legacy.EmailConfirmed = true;
        await userManager.UpdateAsync(legacy);
    }
}
catch (Exception ex)
{
    startupLogger.LogCritical(
        ex,
        "Startup failed while connecting or migrating the database. " +
        "Check ConnectionStrings__DefaultConnection on MonsterASP. " +
        "If the password contains # wrap it in double quotes, e.g. Password=\"your#password\".");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/favicon.ico", (IWebHostEnvironment env) =>
{
    var iconPath = Path.Combine(env.WebRootPath, "SpendiTrackIcon.svg");
    return File.Exists(iconPath)
        ? Results.File(iconPath, "image/svg+xml")
        : Results.NotFound();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static string DescribeConnectionTarget(string connectionString)
{
    try
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        return $"{builder.DataSource}/{builder.InitialCatalog}";
    }
    catch
    {
        return "(unparseable connection string)";
    }
}
