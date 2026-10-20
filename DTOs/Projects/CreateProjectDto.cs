using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Projects
{
    public class CreateProjectDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
