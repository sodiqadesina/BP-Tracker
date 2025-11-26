using BPA1.Data;
using BPA1.Models;
using BPA1.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// MVC + FluentValidation
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

// DbContext with SQL Server (dev)
var connectstr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectstr, sqlOptions =>
    {
        // Retry on transient SQL errors (useful for Azure SQL/serverless waking up)
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();

// App Services
builder.Services.AddScoped<IMeasurementService, MeasurementService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed database (test user + sample data)
await BPA1.Data.DbSeeder.SeedAsync(app.Services);

// Seed default user and sample data
await SeedData.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- Development seed: default user + sample measurements ---
using (var scope = app.Services.CreateScope())
{
    try
    {
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (env.IsDevelopment())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure Positions are present (already via OnModelCreating)

            // Ensure test user exists
            var email = "test@yahoo.com";
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, "Password123.");
                if (!result.Succeeded)
                {
                    Console.WriteLine("Failed to create seed user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            if (user != null)
            {
                // Seed ~20 measurements spanning categories if not present
                var existing = db.BpMeasurements.Count(m => m.UserId == user.Id);
                if (existing < 20)
                {
                    var now = DateTime.UtcNow.Date;
                    // Curated values to cover all categories
                    var seeds = new (int sys, int dia)[] {
                        (112, 72), (115, 78), (118, 76), // Normal
                        (121, 76), (125, 77), (128, 79), // Elevated
                        (131, 78), (135, 82), (138, 85), // Stage 1
                        (142, 92), (150, 95), (160, 100), // Stage 2
                        (181, 110), (175, 122), (185, 125), // Crisis
                        (119, 79), (129, 70), (133, 75), (145, 88), (178, 119)
                    };
                    var posIds = db.Positions.Select(p => p.Id).OrderBy(x => x).ToArray();
                    if (posIds.Length == 0) { posIds = new int[] { 1, 2, 3 }; }

                    var list = new List<BpMeasurement>();
                    for (int i = 0; i < seeds.Length; i++)
                    {
                        var (s, d) = seeds[i];
                        list.Add(new BpMeasurement
                        {
                            UserId = user.Id,
                            Systolic = s,
                            Diastolic = d,
                            DateOfMeasurement = now.AddDays(-i * 2),
                            Pulse = 60 + (i % 5) * 5,
                            Notes = "Seeded sample",
                            PositionId = posIds[i % posIds.Length]
                        });
                    }
                    db.BpMeasurements.AddRange(list);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seeding error: " + ex.Message);
    }
}
// --- End seed ---

app.Run();
