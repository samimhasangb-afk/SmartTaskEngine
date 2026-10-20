using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Labels;
using SmartTaskEngine.DTOs.Tasks;
using SmartTaskEngine.Models;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _db;
        private readonly TaskAccess _access;
        private readonly INotificationService _notifications;

        public TaskService(ApplicationDbContext db, TaskAccess access, INotificationService notifications)
        {
            _db = db;
            _access = access;
            _notifications = notifications;
        }

        public async Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, TaskState? status, TaskPriority? priority, string? search, int? projectId, bool bookmarkedOnly = false)
        {
            var query = _access.VisibleTasks(currentUserId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (bookmarkedOnly)
                query = query.Where(t => t.Bookmarks.Any(b => b.UserId == currentUserId));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

            var tasks = await _access.WithDetails(query).OrderByDescending(t => t.CreatedAt).ToListAsync();
            return tasks.Select(t => MapToDto(t, currentUserId)).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id, int currentUserId)
        {
            var task = await _access.WithDetails(_access.VisibleTasks(currentUserId))
                .FirstOrDefaultAsync(t => t.Id == id);
            return task == null ? null : MapToDto(task, currentUserId);
        }

        public async Task<TaskResponseDto?> CreateTaskAsync(CreateTaskDto dto, int currentUserId)
        {
            if (dto.ProjectId.HasValue && !await _access.CanAccessProjectAsync(dto.ProjectId.Value, currentUserId))
                return null;

            if (dto.AssignedToUserId.HasValue && !await _db.Users.AnyAsync(u => u.Id == dto.AssignedToUserId.Value))
                return null;

            var task = new TodoTask
            {
                Title = dto.Title.Trim(),
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                AssignedToUserId = dto.AssignedToUserId,
                ProjectId = dto.ProjectId,
                CreatedByUserId = currentUserId,
                Status = TaskState.ToDo
            };

            _db.TodoTasks.Add(task);
            await _db.SaveChangesAsync();
            await AddLogAsync(task.Id, currentUserId, "Task created");

            if (dto.AssignedToUserId.HasValue)
            {
                await _notifications.NotifyAsync(
                    dto.AssignedToUserId.Value,
                    currentUserId,
                    task.Id,
                    task.ProjectId,
                    NotificationType.Assigned,
                    $"You were assigned \"{task.Title}\".");
            }

            return await GetTaskByIdAsync(task.Id, currentUserId);
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId)
        {
            var task = await _access.VisibleTasks(currentUserId).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return null;

            var changes = new List<string>();
            var statusChanged = false;

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
                statusChanged = true;
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
                    await AddLogAsync(task.Id, currentUserId, change);
            }

            if (statusChanged && task.AssignedToUserId.HasValue)
            {
                await _notifications.NotifyAsync(
                    task.AssignedToUserId.Value,
                    currentUserId,
                    task.Id,
                    task.ProjectId,
                    NotificationType.StatusChanged,
                    $"\"{task.Title}\" moved to {task.Status}.");
            }

            return await GetTaskByIdAsync(task.Id, currentUserId);
        }

        public async Task<TaskResponseDto?> MoveTaskAsync(int id, int? projectId, int currentUserId)
        {
            var task = await _access.VisibleTasks(currentUserId).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return null;

            if (projectId.HasValue && !await _access.CanAccessProjectAsync(projectId.Value, currentUserId))
                return null;

            task.ProjectId = projectId;
            await _db.SaveChangesAsync();
            await AddLogAsync(task.Id, currentUserId, projectId.HasValue ? $"Moved to project #{projectId}" : "Removed from project");
            return await GetTaskByIdAsync(task.Id, currentUserId);
        }

        public async Task<bool> DeleteTaskAsync(int id, int currentUserId)
        {
            var task = await _access.VisibleTasks(currentUserId).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return false;

            await AddLogAsync(task.Id, currentUserId, "Task deleted");
            _db.TodoTasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TaskResponseDto?> AssignTaskAsync(int id, AssignTaskDto dto, int currentUserId)
        {
            var task = await _access.VisibleTasks(currentUserId).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return null;

            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.AssignedToUserId);
            if (!userExists) return null;

            task.AssignedToUserId = dto.AssignedToUserId;
            await _db.SaveChangesAsync();
            await AddLogAsync(task.Id, currentUserId, $"Task assigned to user #{dto.AssignedToUserId}");
            await _notifications.NotifyAsync(
                dto.AssignedToUserId,
                currentUserId,
                task.Id,
                task.ProjectId,
                NotificationType.Assigned,
                $"You were assigned \"{task.Title}\".");

            return await GetTaskByIdAsync(task.Id, currentUserId);
        }

        public async Task<List<ActivityLogDto>?> GetLogsAsync(int taskId, int currentUserId)
        {
            var visible = await _access.VisibleTasks(currentUserId).AnyAsync(t => t.Id == taskId);
            if (!visible) return null;

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

        private static TaskResponseDto MapToDto(TodoTask t, int currentUserId) => new()
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
            AssignedToUsername = t.AssignedToUser?.Username,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name,
            IsBookmarked = t.Bookmarks.Any(b => b.UserId == currentUserId),
            CommentCount = t.Comments.Count,
            ChecklistTotal = t.ChecklistItems.Count,
            ChecklistDone = t.ChecklistItems.Count(i => i.IsDone),
            Labels = t.Labels
                .OrderBy(l => l.Name)
                .Select(l => new LabelDto { Id = l.Id, Name = l.Name, Color = l.Color })
                .ToList()
        };
    }
}
