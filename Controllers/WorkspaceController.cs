using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskEngine.Helpers;
using SmartTaskEngine.Services;

namespace SmartTaskEngine.Controllers
{
    [ApiController]
    [Authorize]
    public class WorkspaceController : ControllerBase
    {
        private readonly INotificationService _notifications;
        private readonly IDashboardService _dashboard;
        private readonly ICollaborationService _collaboration;

        public WorkspaceController(INotificationService notifications, IDashboardService dashboard, ICollaborationService collaboration)
        {
            _notifications = notifications;
            _dashboard = dashboard;
            _collaboration = collaboration;
        }

        [HttpGet("api/notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly) =>
            Ok(await _notifications.GetAsync(CurrentUser.Id(User), unreadOnly));

        [HttpPatch("api/notifications/{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var ok = await _notifications.MarkReadAsync(id, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("api/notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var count = await _notifications.MarkAllReadAsync(CurrentUser.Id(User));
            return Ok(new { marked = count });
        }

        [HttpGet("api/dashboard")]
        public async Task<IActionResult> Dashboard() =>
            Ok(await _dashboard.GetAsync(CurrentUser.Id(User)));

        [HttpGet("api/users")]
        public async Task<IActionResult> Users() =>
            Ok(await _dashboard.GetUsersAsync());

        [HttpGet("api/bookmarks")]
        public async Task<IActionResult> Bookmarks() =>
            Ok(await _collaboration.GetBookmarksAsync(CurrentUser.Id(User)));
    }
}
