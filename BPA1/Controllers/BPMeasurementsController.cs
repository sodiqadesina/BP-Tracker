using BPA1.Data;
using BPA1.Models;
using BPA1.Services;
using BPA1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BPA1.Controllers
{
    [Authorize]
    public class BPMeasurementsController : Controller
    {
        private readonly IMeasurementService _svc;
        private readonly ApplicationDbContext _db;
        public BPMeasurementsController(IMeasurementService svc, ApplicationDbContext db) { _svc = svc; _db = db; }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private async Task PopulatePositionsAsync()
        {
            var positions = await _db.Positions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            ViewBag.PositionId = new SelectList(positions, "Id", "Name");
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] MeasurementFilterVm filter, CancellationToken ct)
        {
            var data = await _svc.ListAsync(UserId, filter, ct);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulatePositionsAsync();
            return View(new MeasurementCreateVm { DateOfMeasurement = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MeasurementCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePositionsAsync();
                return View(vm);
            }
            await _svc.CreateAsync(UserId, vm, ct);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            // Load the existing entity for this user
            var entity = await _db.BpMeasurements.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId && !x.IsDeleted, ct);
            if (entity == null) return NotFound();

            await PopulatePositionsAsync();
            var vm = new MeasurementCreateVm
            {
                Systolic = entity.Systolic,
                Diastolic = entity.Diastolic,
                DateOfMeasurement = entity.DateOfMeasurement,
                Pulse = entity.Pulse,
                Notes = entity.Notes,
                PositionId = entity.PositionId
            };
            ViewData["Id"] = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MeasurementCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePositionsAsync();
                ViewData["Id"] = id;
                return View(vm);
            }
            var ok = await _svc.UpdateAsync(UserId, id, vm, ct);
            if (!ok) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _svc.DeleteAsync(UserId, id, ct);
            if (!ok) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        // Dashboard
        [HttpGet]
        public IActionResult Dashboard() => View();

        [HttpGet("bp/api/trend")]
        
public async Task<IActionResult> Trend([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
{
    var list = await _svc.TrendAsync(UserId, from, to, ct);
    var shaped = list.Select(x => new { date = x.date, systolic = x.sys, diastolic = x.dia });
    return Ok(shaped);
}
[HttpGet("bp/api/categories")]
        public async Task<IActionResult> Categories([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
            => Ok(await _svc.CategoryBreakdownAsync(UserId, from, to, ct));
    }
}