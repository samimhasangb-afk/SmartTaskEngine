using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskState Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUsername { get; set; }
    }
}
