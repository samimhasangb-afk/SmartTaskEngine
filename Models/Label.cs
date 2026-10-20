using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskEngine.Models
{
    public class Label
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Color { get; set; } = "#64748b";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
