using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Notifications;
using SmartTaskEngine.Models;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(int recipientUserId, int? actorUserId, int? taskId, int? projectId, NotificationType type, string message);
        Task<List<NotificationDto>> GetAsync(int userId, bool? unreadOnly);
        Task<bool> MarkReadAsync(int id, int userId);
        Task<int> MarkAllReadAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAsync(int recipientUserId, int? actorUserId, int? taskId, int? projectId, NotificationType type, string message)
        {
            if (actorUserId.HasValue && recipientUserId == actorUserId.Value)
                return;

            var exists = await _db.Users.AnyAsync(u => u.Id == recipientUserId);
            if (!exists) return;

            _db.Notifications.Add(new Notification
            {
                UserId = recipientUserId,
                ActorUserId = actorUserId,
                TaskId = taskId,
                ProjectId = projectId,
                Type = type,
                Message = message.Length > 300 ? message[..300] : message,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetAsync(int userId, bool? unreadOnly)
        {
            var query = _db.Notifications
                .Include(n => n.Actor)
                .Where(n => n.UserId == userId);

            if (unreadOnly == true)
                query = query.Where(n => !n.IsRead);

            var items = await query.OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync();
            return items.Select(Map).ToList();
        }

        public async Task<bool> MarkReadAsync(int id, int userId)
        {
            var item = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (item == null) return false;
            item.IsRead = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllReadAsync(int userId)
        {
            var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var item in unread)
                item.IsRead = true;
            await _db.SaveChangesAsync();
            return unread.Count;
        }

        private static NotificationDto Map(Notification n) => new()
        {
            Id = n.Id,
            Type = n.Type,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ActorUserId = n.ActorUserId,
            ActorUsername = n.Actor?.Username,
            TaskId = n.TaskId,
            ProjectId = n.ProjectId
        };
    }
}
