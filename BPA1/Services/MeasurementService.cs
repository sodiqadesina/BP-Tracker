using BPA1.Data;
using BPA1.Models;
using BPA1.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BPA1.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly ApplicationDbContext _db;
        public MeasurementService(ApplicationDbContext db) { _db = db; }

        private static string CategoryClassMap(string category) => category switch
        {
            "Normal" => "bg-success",
            "Elevated" => "bg-warning text-dark",
            "Stage 1" => "bg-info text-dark",
            "Stage 2" => "bg-danger",
            "Hypertensive Crisis" => "bg-dark",
            _ => "bg-secondary"
        };

        public async Task<PagedResult<MeasurementListItemVm>> ListAsync(string userId, MeasurementFilterVm filter, CancellationToken ct)
        {
            var q = _db.BpMeasurements
                .AsNoTracking()
                .Include(x => x.Position)
                .Where(x => x.UserId == userId && !x.IsDeleted);

            if (filter.From.HasValue) q = q.Where(x => x.DateOfMeasurement >= filter.From);
            if (filter.To.HasValue) q = q.Where(x => x.DateOfMeasurement <= filter.To);
            if (filter.MinSys.HasValue) q = q.Where(x => x.Systolic >= filter.MinSys);
            if (filter.MaxSys.HasValue) q = q.Where(x => x.Systolic <= filter.MaxSys);
            if (filter.MinDia.HasValue) q = q.Where(x => x.Diastolic >= filter.MinDia);
            if (filter.MaxDia.HasValue) q = q.Where(x => x.Diastolic <= filter.MaxDia);
            if (!string.IsNullOrWhiteSpace(filter.Category))
                q = q.Where(x => BpMeasurement.Categorize(x.Systolic, x.Diastolic) == filter.Category);

            q = (filter.SortBy?.ToLower()) switch
            {
                "systolic" => filter.Desc ? q.OrderByDescending(x => x.Systolic) : q.OrderBy(x => x.Systolic),
                "diastolic" => filter.Desc ? q.OrderByDescending(x => x.Diastolic) : q.OrderBy(x => x.Diastolic),
                _ => filter.Desc ? q.OrderByDescending(x => x.DateOfMeasurement) : q.OrderBy(x => x.DateOfMeasurement)
            };

            var total = await q.CountAsync(ct);
            var items = await q.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(x => new MeasurementListItemVm
                {
                    Id = x.Id,
                    Systolic = x.Systolic,
                    Diastolic = x.Diastolic,
                    DateOfMeasurement = x.DateOfMeasurement,
                    Category = BpMeasurement.Categorize(x.Systolic, x.Diastolic),
                    CategoryClass = CategoryClassMap(BpMeasurement.Categorize(x.Systolic, x.Diastolic)),
                    Position = x.Position != null ? x.Position.Name : null,
                    Pulse = x.Pulse,
                    Notes = x.Notes
                }).ToListAsync(ct);

            return new PagedResult<MeasurementListItemVm>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = total
            };
        }

        public async Task<int> CreateAsync(string userId, MeasurementCreateVm vm, CancellationToken ct)
        {
            var entity = new BpMeasurement
            {
                UserId = userId,
                Systolic = vm.Systolic,
                Diastolic = vm.Diastolic,
                DateOfMeasurement = vm.DateOfMeasurement,
                Pulse = vm.Pulse,
                Notes = vm.Notes,
                PositionId = vm.PositionId
            };
            _db.BpMeasurements.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string userId, int id, MeasurementCreateVm vm, CancellationToken ct)
        {
            var entity = await _db.BpMeasurements.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted, ct);
            if (entity == null) return false;
            entity.Systolic = vm.Systolic;
            entity.Diastolic = vm.Diastolic;
            entity.DateOfMeasurement = vm.DateOfMeasurement;
            entity.Pulse = vm.Pulse;
            entity.Notes = vm.Notes;
            entity.PositionId = vm.PositionId;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(string userId, int id, CancellationToken ct)
        {
            var entity = await _db.BpMeasurements.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted, ct);
            if (entity == null) return false;
            entity.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<Dictionary<string, int>> CategoryBreakdownAsync(string userId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var q = _db.BpMeasurements.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted);
            if (from.HasValue) q = q.Where(x => x.DateOfMeasurement >= from);
            if (to.HasValue) q = q.Where(x => x.DateOfMeasurement <= to);
            var list = await q.ToListAsync(ct);
            return list.GroupBy(x => BpMeasurement.Categorize(x.Systolic, x.Diastolic))
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<List<(DateTime date, int sys, int dia)>> TrendAsync(string userId, DateTime? from, DateTime? to, CancellationToken ct)
        {
            var q = _db.BpMeasurements.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted);
            if (from.HasValue) q = q.Where(x => x.DateOfMeasurement >= from);
            if (to.HasValue) q = q.Where(x => x.DateOfMeasurement <= to);
            return await q.OrderBy(x => x.DateOfMeasurement)
                .Select(x => new ValueTuple<DateTime,int,int>(x.DateOfMeasurement, x.Systolic, x.Diastolic))
                .ToListAsync(ct);
        }

        public async Task<List<(int id, string name)>> GetPositionsAsync(CancellationToken ct)
        {
            return await _db.Positions
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ValueTuple<int,string>(p.Id, p.Name))
                .ToListAsync(ct);
        }
    }
}