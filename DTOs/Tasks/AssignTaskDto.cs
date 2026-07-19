using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Tasks
{
    public class AssignTaskDto
    {
        [Required]
        public int AssignedToUserId { get; set; }
    }
}
