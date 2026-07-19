using System.ComponentModel.DataAnnotations;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public TaskState? Status { get; set; }

        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
