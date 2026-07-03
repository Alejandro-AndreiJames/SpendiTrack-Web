using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using SpendiTrackWeb.Data;
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<SpendiTrackWeb.Services.BudgetCalculator>();
builder.Services.AddScoped<SpendiTrackWeb.Services.MonthlyBudgetService>();
builder.Services.AddScoped<SpendiTrackWeb.Services.TrackerAccessService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
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
