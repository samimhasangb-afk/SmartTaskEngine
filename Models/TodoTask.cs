using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Models
{
    public class TodoTask
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public TaskState Status { get; set; } = TaskState.ToDo;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        [ForeignKey(nameof(AssignedToUser))]
        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }

        [ForeignKey(nameof(Project))]
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
        public ICollection<Label> Labels { get; set; } = new List<Label>();
        public ICollection<TaskBookmark> Bookmarks { get; set; } = new List<TaskBookmark>();
    }
}
