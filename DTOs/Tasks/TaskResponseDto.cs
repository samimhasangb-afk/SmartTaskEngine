using SmartTaskEngine.DTOs.Labels;
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
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public bool IsBookmarked { get; set; }
        public int CommentCount { get; set; }
        public int ChecklistTotal { get; set; }
        public int ChecklistDone { get; set; }
        public List<LabelDto> Labels { get; set; } = new();
    }
}
