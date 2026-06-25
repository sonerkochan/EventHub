using System.Globalization;
using Elastic.Apm.NetCoreAll;
using EventHub.Core.Models.Travelis;
using EventHub.Localization;
using EventHub.Infrastructure.Data;
using EventHub.Infrastructure.Data.Models;
using EventHub.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Environment.EnvironmentName switch
    {
        "Production" => builder.Configuration.GetConnectionString("ProdConnection"),
        "Staging" => builder.Configuration.GetConnectionString("StagingConnection"),
        _ => builder.Configuration.GetConnectionString("DevConnection"),
    } ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder
    .Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = false;
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Login";
    options.LogoutPath = "/User/Logout";
});

// Add localization support
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddApplicationServices();
builder.Services.Configure<TravelisOptions>(builder.Configuration.GetSection(TravelisOptions.Section));
builder.Services.AddStripe(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = SupportedCultures.GetCultures();

    options.DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(ValidationResource));
    });
builder.Services.AddAuthorization();

var app = builder.Build();

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
app.UseStaticFiles();

app.UseMiddleware<AnalyticsMiddleware>();
app.UseRouting();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseAuthentication();
app.UseAuthorization();

app.UseAllElasticApm();

// After Auth
#pragma warning disable ASP0014 // disabled just cuz it was bitchy
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}"
    );
    endpoints.MapRazorPages();
});
#pragma warning restore ASP0014

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

app.Run();
