using System.ComponentModel.DataAnnotations;

namespace SmartTaskEngine.DTOs.Comments
{
    public class CreateCommentDto
    {
        [Required, MaxLength(2000)]
        public string Body { get; set; } = string.Empty;
    }
}
