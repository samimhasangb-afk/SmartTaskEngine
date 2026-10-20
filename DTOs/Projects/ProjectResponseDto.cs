namespace SmartTaskEngine.DTOs.Projects
{
    public class ProjectMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ProjectResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }
        public int TaskCount { get; set; }
        public List<ProjectMemberDto> Members { get; set; } = new();
    }
}
