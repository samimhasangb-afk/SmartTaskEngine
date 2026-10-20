using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskEngine.Models
{
    public class TaskBookmark
    {
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey(nameof(Task))]
        public int TaskId { get; set; }
        public TodoTask? Task { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
