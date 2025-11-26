
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BPA1.Models
{
    public class BpMeasurement
    {
        public int Id { get; set; }

        [Range(20, 400)]
        public int Systolic { get; set; }

        [Range(10, 300)]
        public int Diastolic { get; set; }

        [Required]
        public DateTime DateOfMeasurement { get; set; }

        public int? Pulse { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }

        // FK
        public int? PositionId { get; set; }
        public Position? Position { get; set; }

        // Multi-tenant
        [Required]
        public string UserId { get; set; } = default!;

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        [NotMapped]
        public string Category => Categorize(Systolic, Diastolic);

        public static string Categorize(int sys, int dia)
        {
            if (sys >= 180 || dia >= 120) return "Hypertensive Crisis";
            if (sys >= 140 || dia >= 90) return "Stage 2";
            if (sys >= 130 || dia >= 80) return "Stage 1";
            if (sys >= 120 && dia < 80) return "Elevated";
            return "Normal";
        }
    }

    public class Position
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
