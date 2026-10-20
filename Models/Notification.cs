using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Recipient))]
        public int UserId { get; set; }
        public User? Recipient { get; set; }

        [ForeignKey(nameof(Actor))]
        public int? ActorUserId { get; set; }
        public User? Actor { get; set; }

        public int? TaskId { get; set; }
        public TodoTask? RelatedTask { get; set; }

        public int? ProjectId { get; set; }
        public Project? RelatedProject { get; set; }

        public NotificationType Type { get; set; }

        [Required, MaxLength(300)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
