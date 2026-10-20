using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskEngine.DTOs.Labels;
using SmartTaskEngine.Helpers;
using SmartTaskEngine.Services;

namespace SmartTaskEngine.Controllers
{
    [ApiController]
    [Route("api/labels")]
    [Authorize]
    public class LabelsController : ControllerBase
    {
        private readonly ICollaborationService _collaboration;

        public LabelsController(ICollaborationService collaboration)
        {
            _collaboration = collaboration;
        }

        [HttpGet]
        public async Task<IActionResult> GetLabels() =>
            Ok(await _collaboration.GetLabelsAsync(CurrentUser.Id(User)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateLabelDto dto)
        {
            var label = await _collaboration.CreateLabelAsync(dto, CurrentUser.Id(User));
            if (label == null) return Conflict(new { message = "You already have a label with that name." });
            return Ok(label);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _collaboration.DeleteLabelAsync(id, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }
    }
}
