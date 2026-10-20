using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskEngine.DTOs.Tasks;
using SmartTaskEngine.Helpers;
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

        // GET /api/tasks?status=ToDo&priority=High&search=report&projectId=1
        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskState? status, [FromQuery] TaskPriority? priority, [FromQuery] string? search, [FromQuery] int? projectId)
        {
            var tasks = await _taskService.GetTasksAsync(CurrentUser.Id(User), status, priority, search, projectId);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id, CurrentUser.Id(User));
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskDto dto)
        {
            var task = await _taskService.CreateTaskAsync(dto, CurrentUser.Id(User));
            if (task == null) return BadRequest(new { message = "Project or assignee was not found." });
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
        {
            var task = await _taskService.UpdateTaskAsync(id, dto, CurrentUser.Id(User));
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPatch("{id}/project")]
        public async Task<IActionResult> MoveTask(int id, MoveTaskDto dto)
        {
            var task = await _taskService.MoveTaskAsync(id, dto.ProjectId, CurrentUser.Id(User));
            if (task == null) return NotFound(new { message = "Task or project not found." });
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var success = await _taskService.DeleteTaskAsync(id, CurrentUser.Id(User));
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTask(int id, AssignTaskDto dto)
        {
            var task = await _taskService.AssignTaskAsync(id, dto, CurrentUser.Id(User));
            if (task == null) return NotFound(new { message = "Task or target user not found." });
            return Ok(task);
        }

        [HttpGet("{id}/logs")]
        public async Task<IActionResult> GetLogs(int id)
        {
            var logs = await _taskService.GetLogsAsync(id, CurrentUser.Id(User));
            return logs == null ? NotFound() : Ok(logs);
        }
    }
}
