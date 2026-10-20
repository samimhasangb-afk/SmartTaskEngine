using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Projects
{
    public class AddProjectMemberDto
    {
        [Required]
        public int UserId { get; set; }
    }
}
