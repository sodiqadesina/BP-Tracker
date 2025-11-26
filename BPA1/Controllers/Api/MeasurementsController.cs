
using BPA1.Services;
using BPA1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BPA1.Controllers.Api
{
    [ApiController]
    [Route("api/measurements")]
    [Authorize]
    public class MeasurementsController : ControllerBase
    {
        private readonly IMeasurementService _svc;
        public MeasurementsController(IMeasurementService svc) { _svc = svc; }
        private string UserId => User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] MeasurementFilterVm filter, CancellationToken ct)
            => Ok(await _svc.ListAsync(UserId, filter, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MeasurementCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var id = await _svc.CreateAsync(UserId, vm, ct);
            return Created($"/api/measurements/{id}", new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MeasurementCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var ok = await _svc.UpdateAsync(UserId, id, vm, ct);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _svc.DeleteAsync(UserId, id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
