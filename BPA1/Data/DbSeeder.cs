using System;
using System.Linq;
using System.Threading.Tasks;
using BPA1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BPA1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            var db = scopedProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure DB exists/migrated
            await db.Database.MigrateAsync();

            // Create test user if missing
            var email = "test@yahoo.com";
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "Password@123");
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create seed user: {errors}");
                }
            }

            // Seed 20 BP measurements if user has none
            if (!await db.BpMeasurements.AnyAsync(m => m.UserId == user.Id))
            {
                var now = DateTime.UtcNow.Date;
                var rnd = new Random(42);
                var samples = new (int sys, int dia)[]
                {
                    (112, 72),  // Normal
                    (118, 76),  // Normal
                    (122, 78),  // Elevated
                    (127, 79),  // Elevated
                    (131, 82),  // Stage 1
                    (135, 85),  // Stage 1
                    (142, 91),  // Stage 2
                    (150, 95),  // Stage 2
                    (181, 110), // Crisis (sys)
                    (175, 121), // Crisis (dia)
                };

                var list = new System.Collections.Generic.List<BpMeasurement>();
                for (int i = 0; i < 20; i++)
                {
                    var pick = samples[i % samples.Length];
                    var date = now.AddDays(-i);
                    var pulse = 60 + rnd.Next(0, 40); // 60-99
                    var posId = 1 + (i % 3); // 1..3

                    list.Add(new BpMeasurement
                    {
                        UserId = user.Id,
                        Systolic = pick.sys,
                        Diastolic = pick.dia,
                        DateOfMeasurement = date,
                        Pulse = pulse,
                        Notes = $"Seeded reading #{i+1}",
                        PositionId = posId,
                        CreatedAt = date
                    });
                }

                db.BpMeasurements.AddRange(list);
                await db.SaveChangesAsync();
            }
        }
    }
}