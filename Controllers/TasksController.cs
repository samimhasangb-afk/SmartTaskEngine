using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskEngine.DTOs.Tasks;
using SmartTaskEngine.Models.Enums;
using SmartTaskEngine.Services;

namespace SmartTaskEngine.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new InvalidOperationException("User id claim missing from token."));

        // GET /api/tasks?status=ToDo&priority=High&search=report
        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskState? status, [FromQuery] TaskPriority? priority, [FromQuery] string? search)
        {
            var tasks = await _taskService.GetTasksAsync(CurrentUserId, status, priority, search);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskDto dto)
        {
            var task = await _taskService.CreateTaskAsync(dto, CurrentUserId);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
        {
            var task = await _taskService.UpdateTaskAsync(id, dto, CurrentUserId);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var success = await _taskService.DeleteTaskAsync(id, CurrentUserId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTask(int id, AssignTaskDto dto)
        {
            var task = await _taskService.AssignTaskAsync(id, dto, CurrentUserId);
            if (task == null) return NotFound(new { message = "Task or target user not found." });
            return Ok(task);
        }

        [HttpGet("{id}/logs")]
        public async Task<IActionResult> GetLogs(int id)
        {
            var logs = await _taskService.GetLogsAsync(id);
            return Ok(logs);
        }
    }
}
