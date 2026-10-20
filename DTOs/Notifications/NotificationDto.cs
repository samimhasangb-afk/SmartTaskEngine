using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.DTOs.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ActorUserId { get; set; }
        public string? ActorUsername { get; set; }
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
    }
}
