using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Checklist
{
    public class CreateChecklistItemDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateChecklistItemDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public bool? IsDone { get; set; }
    }

    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
