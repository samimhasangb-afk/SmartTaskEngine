using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskEngine.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
