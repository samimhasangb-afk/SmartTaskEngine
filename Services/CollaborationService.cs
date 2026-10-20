using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Checklist;
using SmartTaskEngine.DTOs.Comments;
using SmartTaskEngine.DTOs.Labels;
using SmartTaskEngine.DTOs.Tasks;
using SmartTaskEngine.Models;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public interface ICollaborationService
    {
        Task<List<CommentResponseDto>?> GetCommentsAsync(int taskId, int userId);
        Task<CommentResponseDto?> AddCommentAsync(int taskId, CreateCommentDto dto, int userId);
        Task<bool> DeleteCommentAsync(int taskId, int commentId, int userId);

        Task<List<LabelDto>> GetLabelsAsync(int userId);
        Task<LabelDto?> CreateLabelAsync(CreateLabelDto dto, int userId);
        Task<bool> DeleteLabelAsync(int labelId, int userId);
        Task<TaskResponseDto?> AttachLabelAsync(int taskId, int labelId, int userId);
        Task<TaskResponseDto?> DetachLabelAsync(int taskId, int labelId, int userId);

        Task<List<ChecklistItemDto>?> GetChecklistAsync(int taskId, int userId);
        Task<ChecklistItemDto?> AddChecklistItemAsync(int taskId, CreateChecklistItemDto dto, int userId);
        Task<ChecklistItemDto?> UpdateChecklistItemAsync(int taskId, int itemId, UpdateChecklistItemDto dto, int userId);
        Task<bool> DeleteChecklistItemAsync(int taskId, int itemId, int userId);

        Task<bool> BookmarkAsync(int taskId, int userId);
        Task<bool> RemoveBookmarkAsync(int taskId, int userId);
        Task<List<TaskResponseDto>> GetBookmarksAsync(int userId);
    }

    public class CollaborationService : ICollaborationService
    {
        private readonly ApplicationDbContext _db;
        private readonly TaskAccess _access;
        private readonly INotificationService _notifications;
        private readonly ITaskService _tasks;

        public CollaborationService(
            ApplicationDbContext db,
            TaskAccess access,
            INotificationService notifications,
            ITaskService tasks)
        {
            _db = db;
            _access = access;
            _notifications = notifications;
            _tasks = tasks;
        }

        public async Task<List<CommentResponseDto>?> GetCommentsAsync(int taskId, int userId)
        {
            if (!await CanSeeTask(taskId, userId)) return null;
            var comments = await _db.TaskComments
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                Username = c.User?.Username,
                Body = c.Body,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<CommentResponseDto?> AddCommentAsync(int taskId, CreateCommentDto dto, int userId)
        {
            var task = await _access.VisibleTasks(userId).FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return null;

            var comment = new TaskComment
            {
                TaskId = taskId,
                UserId = userId,
                Body = dto.Body.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _db.TaskComments.Add(comment);
            await _db.SaveChangesAsync();

            var actor = await _db.Users.FindAsync(userId);
            var message = $"{actor?.Username ?? "Someone"} commented on \"{task.Title}\".";
            if (task.CreatedByUserId != userId)
                await _notifications.NotifyAsync(task.CreatedByUserId, userId, taskId, task.ProjectId, NotificationType.CommentAdded, message);
            if (task.AssignedToUserId.HasValue)
                await _notifications.NotifyAsync(task.AssignedToUserId.Value, userId, taskId, task.ProjectId, NotificationType.CommentAdded, message);

            return new CommentResponseDto
            {
                Id = comment.Id,
                TaskId = taskId,
                UserId = userId,
                Username = actor?.Username,
                Body = comment.Body,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(int taskId, int commentId, int userId)
        {
            var comment = await _db.TaskComments.FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId && c.UserId == userId);
            if (comment == null) return false;
            _db.TaskComments.Remove(comment);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<LabelDto>> GetLabelsAsync(int userId)
        {
            return await _db.Labels
                .Where(l => l.CreatedByUserId == userId)
                .OrderBy(l => l.Name)
                .Select(l => new LabelDto { Id = l.Id, Name = l.Name, Color = l.Color })
                .ToListAsync();
        }

        public async Task<LabelDto?> CreateLabelAsync(CreateLabelDto dto, int userId)
        {
            var name = dto.Name.Trim();
            var exists = await _db.Labels.AnyAsync(l => l.CreatedByUserId == userId && l.Name == name);
            if (exists) return null;

            var label = new Label
            {
                Name = name,
                Color = string.IsNullOrWhiteSpace(dto.Color) ? "#64748b" : dto.Color.Trim(),
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Labels.Add(label);
            await _db.SaveChangesAsync();
            return new LabelDto { Id = label.Id, Name = label.Name, Color = label.Color };
        }

        public async Task<bool> DeleteLabelAsync(int labelId, int userId)
        {
            var label = await _db.Labels.FirstOrDefaultAsync(l => l.Id == labelId && l.CreatedByUserId == userId);
            if (label == null) return false;
            _db.Labels.Remove(label);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<TaskResponseDto?> AttachLabelAsync(int taskId, int labelId, int userId)
        {
            var task = await _access.WithDetails(_access.VisibleTasks(userId)).FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return null;

            var label = await _db.Labels.FirstOrDefaultAsync(l => l.Id == labelId && l.CreatedByUserId == userId);
            if (label == null) return null;

            if (task.Labels.All(l => l.Id != labelId))
            {
                task.Labels.Add(label);
                await _db.SaveChangesAsync();
            }

            return await _tasks.GetTaskByIdAsync(taskId, userId);
        }

        public async Task<TaskResponseDto?> DetachLabelAsync(int taskId, int labelId, int userId)
        {
            var task = await _access.WithDetails(_access.VisibleTasks(userId)).FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return null;

            var label = task.Labels.FirstOrDefault(l => l.Id == labelId);
            if (label != null)
            {
                task.Labels.Remove(label);
                await _db.SaveChangesAsync();
            }

            return await _tasks.GetTaskByIdAsync(taskId, userId);
        }

        public async Task<List<ChecklistItemDto>?> GetChecklistAsync(int taskId, int userId)
        {
            if (!await CanSeeTask(taskId, userId)) return null;
            var items = await _db.ChecklistItems
                .Where(i => i.TaskId == taskId)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .ToListAsync();
            return items.Select(MapItem).ToList();
        }

        public async Task<ChecklistItemDto?> AddChecklistItemAsync(int taskId, CreateChecklistItemDto dto, int userId)
        {
            if (!await CanSeeTask(taskId, userId)) return null;
            var nextOrder = await _db.ChecklistItems.Where(i => i.TaskId == taskId).Select(i => (int?)i.SortOrder).MaxAsync() ?? 0;
            var item = new ChecklistItem
            {
                TaskId = taskId,
                Title = dto.Title.Trim(),
                SortOrder = nextOrder + 1,
                CreatedAt = DateTime.UtcNow
            };
            _db.ChecklistItems.Add(item);
            await _db.SaveChangesAsync();
            return MapItem(item);
        }

        public async Task<ChecklistItemDto?> UpdateChecklistItemAsync(int taskId, int itemId, UpdateChecklistItemDto dto, int userId)
        {
            if (!await CanSeeTask(taskId, userId)) return null;
            var item = await _db.ChecklistItems.FirstOrDefaultAsync(i => i.Id == itemId && i.TaskId == taskId);
            if (item == null) return null;

            var becameDone = dto.IsDone == true && !item.IsDone;
            if (dto.Title != null) item.Title = dto.Title.Trim();
            if (dto.IsDone.HasValue) item.IsDone = dto.IsDone.Value;
            await _db.SaveChangesAsync();

            if (becameDone)
            {
                var task = await _db.TodoTasks.FindAsync(taskId);
                var actor = await _db.Users.FindAsync(userId);
                if (task != null)
                {
                    await _notifications.NotifyAsync(
                        task.CreatedByUserId,
                        userId,
                        taskId,
                        task.ProjectId,
                        NotificationType.ChecklistDone,
                        $"{actor?.Username ?? "Someone"} completed \"{item.Title}\" on \"{task.Title}\".");
                }
            }

            return MapItem(item);
        }

        public async Task<bool> DeleteChecklistItemAsync(int taskId, int itemId, int userId)
        {
            if (!await CanSeeTask(taskId, userId)) return false;
            var item = await _db.ChecklistItems.FirstOrDefaultAsync(i => i.Id == itemId && i.TaskId == taskId);
            if (item == null) return false;
            _db.ChecklistItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BookmarkAsync(int taskId, int userId)
        {
            if (!await CanSeeTask(taskId, userId)) return false;
            var exists = await _db.TaskBookmarks.AnyAsync(b => b.UserId == userId && b.TaskId == taskId);
            if (exists) return true;
            _db.TaskBookmarks.Add(new TaskBookmark { UserId = userId, TaskId = taskId, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveBookmarkAsync(int taskId, int userId)
        {
            var bookmark = await _db.TaskBookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.TaskId == taskId);
            if (bookmark == null) return false;
            _db.TaskBookmarks.Remove(bookmark);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<List<TaskResponseDto>> GetBookmarksAsync(int userId) =>
            _tasks.GetTasksAsync(userId, null, null, null, null, bookmarkedOnly: true);

        private Task<bool> CanSeeTask(int taskId, int userId) =>
            _access.VisibleTasks(userId).AnyAsync(t => t.Id == taskId);

        private static ChecklistItemDto MapItem(ChecklistItem item) => new()
        {
            Id = item.Id,
            TaskId = item.TaskId,
            Title = item.Title,
            IsDone = item.IsDone,
            SortOrder = item.SortOrder,
            CreatedAt = item.CreatedAt
        };
    }
}
