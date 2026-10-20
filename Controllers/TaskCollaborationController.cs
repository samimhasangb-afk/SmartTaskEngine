using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskEngine.DTOs.Checklist;
using SmartTaskEngine.DTOs.Comments;
using SmartTaskEngine.Helpers;
using SmartTaskEngine.Services;

namespace SmartTaskEngine.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId:int}")]
    [Authorize]
    public class TaskCollaborationController : ControllerBase
    {
        private readonly ICollaborationService _collaboration;

        public TaskCollaborationController(ICollaborationService collaboration)
        {
            _collaboration = collaboration;
        }

        [HttpGet("comments")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var comments = await _collaboration.GetCommentsAsync(taskId, CurrentUser.Id(User));
            return comments == null ? NotFound() : Ok(comments);
        }

        [HttpPost("comments")]
        public async Task<IActionResult> AddComment(int taskId, CreateCommentDto dto)
        {
            var comment = await _collaboration.AddCommentAsync(taskId, dto, CurrentUser.Id(User));
            return comment == null ? NotFound() : Ok(comment);
        }

        [HttpDelete("comments/{commentId:int}")]
        public async Task<IActionResult> DeleteComment(int taskId, int commentId)
        {
            var ok = await _collaboration.DeleteCommentAsync(taskId, commentId, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("labels/{labelId:int}")]
        public async Task<IActionResult> AttachLabel(int taskId, int labelId)
        {
            var task = await _collaboration.AttachLabelAsync(taskId, labelId, CurrentUser.Id(User));
            return task == null ? NotFound() : Ok(task);
        }

        [HttpDelete("labels/{labelId:int}")]
        public async Task<IActionResult> DetachLabel(int taskId, int labelId)
        {
            var task = await _collaboration.DetachLabelAsync(taskId, labelId, CurrentUser.Id(User));
            return task == null ? NotFound() : Ok(task);
        }

        [HttpGet("checklist")]
        public async Task<IActionResult> GetChecklist(int taskId)
        {
            var items = await _collaboration.GetChecklistAsync(taskId, CurrentUser.Id(User));
            return items == null ? NotFound() : Ok(items);
        }

        [HttpPost("checklist")]
        public async Task<IActionResult> AddChecklistItem(int taskId, CreateChecklistItemDto dto)
        {
            var item = await _collaboration.AddChecklistItemAsync(taskId, dto, CurrentUser.Id(User));
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPatch("checklist/{itemId:int}")]
        public async Task<IActionResult> UpdateChecklistItem(int taskId, int itemId, UpdateChecklistItemDto dto)
        {
            var item = await _collaboration.UpdateChecklistItemAsync(taskId, itemId, dto, CurrentUser.Id(User));
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("checklist/{itemId:int}")]
        public async Task<IActionResult> DeleteChecklistItem(int taskId, int itemId)
        {
            var ok = await _collaboration.DeleteChecklistItemAsync(taskId, itemId, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("bookmark")]
        public async Task<IActionResult> Bookmark(int taskId)
        {
            var ok = await _collaboration.BookmarkAsync(taskId, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("bookmark")]
        public async Task<IActionResult> RemoveBookmark(int taskId)
        {
            var ok = await _collaboration.RemoveBookmarkAsync(taskId, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }
    }
}
