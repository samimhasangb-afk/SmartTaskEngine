using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskEngine.Models
{
    public class ChecklistItem
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Task))]
        public int TaskId { get; set; }
        public TodoTask? Task { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
