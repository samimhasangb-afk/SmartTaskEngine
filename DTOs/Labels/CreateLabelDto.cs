using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Labels
{
    public class CreateLabelDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Color { get; set; }
    }
}
