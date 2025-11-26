
using BPA1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BPA1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<BpMeasurement> BpMeasurements => Set<BpMeasurement>();
        public DbSet<Position> Positions => Set<Position>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Position>().HasData(
                new Position { Id = 1, Name = "Sitting" },
                new Position { Id = 2, Name = "Standing" },
                new Position { Id = 3, Name = "Lying" }
            );
        }
    }
}
