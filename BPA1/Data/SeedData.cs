using System;
using System.Linq;
using System.Threading.Tasks;
using BPA1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BPA1.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var db = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure DB is created/migrated
            await db.Database.MigrateAsync();

            // Ensure Positions exist (in case migrations weren't run with seeding)
            if (!await db.Positions.AnyAsync())
            {
                db.Positions.AddRange(
                    new Position { Id = 1, Name = "Sitting" },
                    new Position { Id = 2, Name = "Standing" },
                    new Position { Id = 3, Name = "Lying" }
                );
                await db.SaveChangesAsync();
            }

            // Create test user if missing
            var email = "test@yahoo.com";
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "Password123");
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception("Failed to create seed user: " + errors);
                }
            }

            // If this user already has measurements, don't reseed
            if (await db.BpMeasurements.AnyAsync(m => m.UserId == user.Id))
                return;

            // Generate 20 readings over past 20 days, covering all categories
            // Categories (rough): Normal, Elevated, Stage 1, Stage 2, Crisis
            var samples = new (int sys, int dia, int? pulse)[]
            {
                (110, 70, 68),   // Normal
                (115, 72, 70),   // Normal
                (125, 75, 72),   // Elevated
                (128, 78, 66),   // Elevated
                (132, 82, 73),   // Stage 1
                (135, 85, 77),   // Stage 1
                (145, 95, 80),   // Stage 2
                (150, 92, 84),   // Stage 2
                (185, 120, 88),  // Crisis
                (190, 110, 91),  // Crisis
                (112, 68, 65),   // Normal
                (118, 76, 71),   // Elevated-ish but stays Elevated rule by sys>=120
                (130, 79, 74),   // Stage 1 (sys>=130)
                (140, 85, 78),   // Stage 2
                (182, 121, 90),  // Crisis
                (124, 79, 69),   // Elevated
                (134, 83, 75),   // Stage 1
                (146, 96, 82),   // Stage 2
                (111, 69, 67),   // Normal
                (188, 122, 93)   // Crisis
            };

            var today = DateTime.Today;
            var positions = new[] { 1, 2, 3 };
            var notes = new[] { "Morning reading", "Evening reading", "Post-walk", "Before meal", "After coffee" };

            for (int i = 0; i < samples.Length; i++)
            {
                var s = samples[i];
                var d = new BpMeasurement
                {
                    UserId = user.Id,
                    Systolic = s.sys,
                    Diastolic = s.dia,
                    DateOfMeasurement = today.AddDays(-i),
                    Pulse = s.pulse,
                    Notes = notes[i % notes.Length],
                    PositionId = positions[i % positions.Length],
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
                };
                db.BpMeasurements.Add(d);
            }

            await db.SaveChangesAsync();
        }
    }
}