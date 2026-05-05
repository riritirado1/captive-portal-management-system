using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CaptivePortal.Web.Data;
using CaptivePortal.Web.Models;
using CaptivePortal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Data Source=captiveportal.db"));

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    // Password settings for admin users
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Wi-Fi Access Service (PBI#5)
builder.Services.Configure<WiFiAccessOptions>(options =>
{
    builder.Configuration.GetSection("CaptivePortal:WiFiAccess").Bind(options);
    // Manually bind RedirectUrls from the separate section
    var redirectSection = builder.Configuration.GetSection("CaptivePortal:RedirectUrls");
    options.RedirectUrls = redirectSection.Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
});

builder.Services.AddScoped<IWiFiAccessService, WiFiAccessService>();

// Add API Controllers
builder.Services.AddControllers();

builder.Services.AddRazorPages(options =>
{
    // Require authentication for admin pages
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeFolder("/Admin");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

// Initialize database and seed admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        // Seed default admin user
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        await SeedAdminUser(userManager);
        
        // Seed sample advertisements
        await SeedSampleAdvertisements(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

async Task SeedAdminUser(UserManager<IdentityUser> userManager)
{
    var adminEmail = "admin@wtamu.edu";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(adminUser, "Admin@123!");
    }
}

async Task SeedSampleAdvertisements(ApplicationDbContext context)
{
    // Check if we already have advertisements
    if (await context.Advertisements.AnyAsync())
        return;

    var sampleAds = new[]
    {
        new Advertisement
        {
            Title = "WTAMU Student Services",
            Description = "Get academic support and student services at our Student Center. Academic advising, tutoring, and career counseling available.",
            ImagePath = "/images/ads/sample-student-services.svg",
            TargetUrl = "https://www.wtamu.edu/student-support",
            Location = "Student Center",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(60)
        },
        new Advertisement
        {
            Title = "WTAMU Library - 24/7 Access",
            Description = "The library is open 24/7 during finals week! Study rooms, research assistance, and online resources available.",
            ImagePath = "/images/ads/sample-library.svg",
            TargetUrl = "https://www.wtamu.edu/library",
            Location = "Library",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow.AddDays(90)
        },
        new Advertisement
        {
            Title = "Campus Dining - Extended Hours",
            Description = "Enjoy fresh meals and grab-and-go options with our new extended dining hours. Buffalo Dining Hall and Union Market now open later!",
            ImagePath = "/images/ads/sample-dining.svg",
            TargetUrl = "https://www.wtamu.edu/dining",
            Location = null, // Show everywhere
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddDays(45)
        }
    };

    await context.Advertisements.AddRangeAsync(sampleAds);
    await context.SaveChangesAsync();

    var logger = app.Services.GetService<ILogger<Program>>();
    logger?.LogInformation("Seeded {Count} sample advertisements", sampleAds.Length);
}
