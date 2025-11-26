
using System.ComponentModel.DataAnnotations;

namespace BPA1.ViewModels
{
    public class MeasurementFilterVm
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? MinSys { get; set; }
        public int? MaxSys { get; set; }
        public int? MinDia { get; set; }
        public int? MaxDia { get; set; }
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Date";
        public bool Desc { get; set; } = true;
    }

    public class MeasurementCreateVm
    {
        [Range(20,400)] public int Systolic { get; set; }
        [Range(10,300)] public int Diastolic { get; set; }
        [Required] public DateTime DateOfMeasurement { get; set; }
        public int? Pulse { get; set; }
        [MaxLength(512)] public string? Notes { get; set; }
        public int? PositionId { get; set; }
    }

    
public class MeasurementListItemVm

    {
        public int Id { get; set; }
        public int Systolic { get; set; }
        public int Diastolic { get; set; }
        public DateTime DateOfMeasurement { get; set; }
        public string Category { get; set; } = "";
        public string CategoryClass { get; set; } = "bg-secondary";
        public string? Position { get; set; }
        public int? Pulse { get; set; }
        public string? Notes { get; set; }
    }

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
