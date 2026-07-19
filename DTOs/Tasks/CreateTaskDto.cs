using System.ComponentModel.DataAnnotations;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.DTOs.Tasks
{
    public class CreateTaskDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public int? AssignedToUserId { get; set; }
    }
}
