using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Tasks;
using SmartTaskEngine.Models;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _db;

        public TaskService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, TaskState? status, TaskPriority? priority, string? search)
        {
            var query = _db.TodoTasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.CreatedByUserId == currentUserId || t.AssignedToUserId == currentUserId)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            return tasks.Select(MapToDto).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id)
        {
            var task = await _db.TodoTasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            return task == null ? null : MapToDto(task);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, int currentUserId)
        {
            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                AssignedToUserId = dto.AssignedToUserId,
                CreatedByUserId = currentUserId,
                Status = TaskState.ToDo
            };

            _db.TodoTasks.Add(task);
            await _db.SaveChangesAsync();

            await AddLogAsync(task.Id, currentUserId, "Task created");

            var saved = await _db.TodoTasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstAsync(t => t.Id == task.Id);

            return MapToDto(saved);
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId)
        {
            var task = await _db.TodoTasks.FindAsync(id);
            if (task == null) return null;

            var changes = new List<string>();

            if (dto.Title != null && dto.Title != task.Title)
            {
                task.Title = dto.Title;
                changes.Add("Title updated");
            }

            if (dto.Description != null && dto.Description != task.Description)
            {
                task.Description = dto.Description;
                changes.Add("Description updated");
            }

            if (dto.Status.HasValue && dto.Status.Value != task.Status)
            {
                task.Status = dto.Status.Value;
                changes.Add($"Status updated to {dto.Status.Value}");
            }

            if (dto.Priority.HasValue && dto.Priority.Value != task.Priority)
            {
                task.Priority = dto.Priority.Value;
                changes.Add($"Priority updated to {dto.Priority.Value}");
            }

            if (dto.DueDate.HasValue && dto.DueDate != task.DueDate)
            {
                task.DueDate = dto.DueDate;
                changes.Add("Due date updated");
            }

            if (changes.Count > 0)
            {
                await _db.SaveChangesAsync();
                foreach (var change in changes)
                {
                    await AddLogAsync(task.Id, currentUserId, change);
                }
            }

            var updated = await _db.TodoTasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstAsync(t => t.Id == task.Id);

            return MapToDto(updated);
        }

        public async Task<bool> DeleteTaskAsync(int id, int currentUserId)
        {
            var task = await _db.TodoTasks.FindAsync(id);
            if (task == null) return false;

            await AddLogAsync(task.Id, currentUserId, "Task deleted");

            _db.TodoTasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TaskResponseDto?> AssignTaskAsync(int id, AssignTaskDto dto, int currentUserId)
        {
            var task = await _db.TodoTasks.FindAsync(id);
            if (task == null) return null;

            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.AssignedToUserId);
            if (!userExists) return null;

            task.AssignedToUserId = dto.AssignedToUserId;
            await _db.SaveChangesAsync();

            await AddLogAsync(task.Id, currentUserId, $"Task assigned to user #{dto.AssignedToUserId}");

            var updated = await _db.TodoTasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstAsync(t => t.Id == task.Id);

            return MapToDto(updated);
        }

        public async Task<List<ActivityLogDto>> GetLogsAsync(int taskId)
        {
            var logs = await _db.ActivityLogs
                .Include(l => l.User)
                .Where(l => l.TaskId == taskId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return logs.Select(l => new ActivityLogDto
            {
                Id = l.Id,
                TaskId = l.TaskId,
                UserId = l.UserId,
                Username = l.User?.Username,
                Action = l.Action,
                Timestamp = l.Timestamp
            }).ToList();
        }

        private async Task AddLogAsync(int taskId, int userId, string action)
        {
            _db.ActivityLogs.Add(new ActivityLog
            {
                TaskId = taskId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        private static TaskResponseDto MapToDto(TodoTask t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            DueDate = t.DueDate,
            CreatedAt = t.CreatedAt,
            CreatedByUserId = t.CreatedByUserId,
            CreatedByUsername = t.CreatedByUser?.Username,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUsername = t.AssignedToUser?.Username
        };
    }
}
