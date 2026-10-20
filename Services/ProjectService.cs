using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Projects;
using SmartTaskEngine.Models;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public interface IProjectService
    {
        Task<List<ProjectResponseDto>> GetProjectsAsync(int userId);
        Task<ProjectResponseDto?> GetProjectAsync(int id, int userId);
        Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, int userId);
        Task<ProjectResponseDto?> UpdateAsync(int id, UpdateProjectDto dto, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<ProjectResponseDto?> AddMemberAsync(int projectId, int memberUserId, int actorUserId);
        Task<bool> RemoveMemberAsync(int projectId, int memberUserId, int actorUserId);
    }

    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _db;
        private readonly TaskAccess _access;
        private readonly INotificationService _notifications;

        public ProjectService(ApplicationDbContext db, TaskAccess access, INotificationService notifications)
        {
            _db = db;
            _access = access;
            _notifications = notifications;
        }

        public async Task<List<ProjectResponseDto>> GetProjectsAsync(int userId)
        {
            var projects = await Query()
                .Where(p => p.CreatedByUserId == userId || p.Members.Any(m => m.UserId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return projects.Select(Map).ToList();
        }

        public async Task<ProjectResponseDto?> GetProjectAsync(int id, int userId)
        {
            if (!await _access.CanAccessProjectAsync(id, userId)) return null;
            var project = await Query().FirstOrDefaultAsync(p => p.Id == id);
            return project == null ? null : Map(project);
        }

        public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, int userId)
        {
            var project = new Project
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            var saved = await Query().FirstAsync(p => p.Id == project.Id);
            return Map(saved);
        }

        public async Task<ProjectResponseDto?> UpdateAsync(int id, UpdateProjectDto dto, int userId)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.CreatedByUserId == userId);
            if (project == null) return null;

            project.Name = dto.Name.Trim();
            project.Description = dto.Description;
            await _db.SaveChangesAsync();

            var saved = await Query().FirstAsync(p => p.Id == project.Id);
            return Map(saved);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.CreatedByUserId == userId);
            if (project == null) return false;
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectResponseDto?> AddMemberAsync(int projectId, int memberUserId, int actorUserId)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.CreatedByUserId == actorUserId);
            if (project == null) return null;
            if (memberUserId == actorUserId) return await GetProjectAsync(projectId, actorUserId);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == memberUserId);
            if (user == null) return null;

            var already = await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == memberUserId);
            if (!already)
            {
                _db.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = memberUserId,
                    JoinedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                await _notifications.NotifyAsync(
                    memberUserId,
                    actorUserId,
                    null,
                    projectId,
                    NotificationType.ProjectInvite,
                    $"You were added to project \"{project.Name}\".");
            }

            return await GetProjectAsync(projectId, actorUserId);
        }

        public async Task<bool> RemoveMemberAsync(int projectId, int memberUserId, int actorUserId)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return false;

            var isOwner = project.CreatedByUserId == actorUserId;
            var removingSelf = memberUserId == actorUserId;
            if (!isOwner && !removingSelf) return false;

            var member = await _db.ProjectMembers.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberUserId);
            if (member == null) return false;

            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync();
            return true;
        }

        private IQueryable<Project> Query() =>
            _db.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.Members).ThenInclude(m => m.User)
                .Include(p => p.Tasks);

        private static ProjectResponseDto Map(Project p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            CreatedByUserId = p.CreatedByUserId,
            CreatedByUsername = p.CreatedByUser?.Username,
            TaskCount = p.Tasks.Count,
            Members = p.Members.Select(m => new ProjectMemberDto
            {
                UserId = m.UserId,
                Username = m.User?.Username ?? string.Empty,
                Email = m.User?.Email ?? string.Empty
            }).ToList()
        };
    }
}
