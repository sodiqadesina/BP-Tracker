using BPA1.ViewModels;

namespace BPA1.Services
{
    public interface IMeasurementService
    {
        Task<PagedResult<MeasurementListItemVm>> ListAsync(string userId, MeasurementFilterVm filter, CancellationToken ct);
        Task<int> CreateAsync(string userId, MeasurementCreateVm vm, CancellationToken ct);
        Task<bool> UpdateAsync(string userId, int id, MeasurementCreateVm vm, CancellationToken ct);
        Task<bool> DeleteAsync(string userId, int id, CancellationToken ct);
        Task<Dictionary<string,int>> CategoryBreakdownAsync(string userId, DateTime? from, DateTime? to, CancellationToken ct);
        Task<List<(DateTime date, int sys, int dia)>> TrendAsync(string userId, DateTime? from, DateTime? to, CancellationToken ct);
        Task<List<(int id, string name)>> GetPositionsAsync(CancellationToken ct);
    }
}