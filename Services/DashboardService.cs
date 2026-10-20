using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.DTOs.Dashboard;
using SmartTaskEngine.DTOs.Users;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync(int userId);
        Task<List<UserSummaryDto>> GetUsersAsync();
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly TaskAccess _access;

        public DashboardService(ApplicationDbContext db, TaskAccess access)
        {
            _db = db;
            _access = access;
        }

        public async Task<DashboardDto> GetAsync(int userId)
        {
            var tasks = _access.VisibleTasks(userId);
            var now = DateTime.UtcNow;

            return new DashboardDto
            {
                TotalTasks = await tasks.CountAsync(),
                ToDo = await tasks.CountAsync(t => t.Status == TaskState.ToDo),
                InProgress = await tasks.CountAsync(t => t.Status == TaskState.InProgress),
                Done = await tasks.CountAsync(t => t.Status == TaskState.Done),
                Overdue = await tasks.CountAsync(t => t.DueDate != null && t.DueDate < now && t.Status != TaskState.Done),
                AssignedToMe = await tasks.CountAsync(t => t.AssignedToUserId == userId && t.Status != TaskState.Done),
                Projects = await _db.Projects.CountAsync(p => p.CreatedByUserId == userId || p.Members.Any(m => m.UserId == userId)),
                OpenChecklistItems = await _db.ChecklistItems.CountAsync(i => !i.IsDone && tasks.Any(t => t.Id == i.TaskId)),
                UnreadNotifications = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
                Bookmarks = await _db.TaskBookmarks.CountAsync(b => b.UserId == userId)
            };
        }

        public async Task<List<UserSummaryDto>> GetUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Username)
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .ToListAsync();
        }
    }
}
