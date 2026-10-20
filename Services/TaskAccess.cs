using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Data;
using SmartTaskEngine.Models;

namespace SmartTaskEngine.Services
{
    public class TaskAccess
    {
        private readonly ApplicationDbContext _db;

        public TaskAccess(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<TodoTask> VisibleTasks(int userId) =>
            _db.TodoTasks.Where(t =>
                t.CreatedByUserId == userId ||
                t.AssignedToUserId == userId ||
                (t.Project != null &&
                    (t.Project.CreatedByUserId == userId ||
                     t.Project.Members.Any(m => m.UserId == userId))));

        public Task<bool> CanAccessProjectAsync(int projectId, int userId) =>
            _db.Projects.AnyAsync(p =>
                p.Id == projectId &&
                (p.CreatedByUserId == userId || p.Members.Any(m => m.UserId == userId)));

        public IQueryable<TodoTask> WithDetails(IQueryable<TodoTask> query) =>
            query
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Project)
                .Include(t => t.Labels)
                .Include(t => t.ChecklistItems)
                .Include(t => t.Comments)
                .Include(t => t.Bookmarks);
    }
}
