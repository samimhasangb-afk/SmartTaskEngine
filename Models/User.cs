using System.ComponentModel.DataAnnotations;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Member;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TodoTask> CreatedTasks { get; set; } = new List<TodoTask>();
        public ICollection<TodoTask> AssignedTasks { get; set; } = new List<TodoTask>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}
