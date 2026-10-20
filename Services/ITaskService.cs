using SmartTaskEngine.DTOs.Tasks;
using SmartTaskEngine.Models.Enums;

namespace SmartTaskEngine.Services
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, TaskState? status, TaskPriority? priority, string? search, int? projectId, bool bookmarkedOnly = false);
        Task<TaskResponseDto?> GetTaskByIdAsync(int id, int currentUserId);
        Task<TaskResponseDto?> CreateTaskAsync(CreateTaskDto dto, int currentUserId);
        Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId);
        Task<TaskResponseDto?> MoveTaskAsync(int id, int? projectId, int currentUserId);
        Task<bool> DeleteTaskAsync(int id, int currentUserId);
        Task<TaskResponseDto?> AssignTaskAsync(int id, AssignTaskDto dto, int currentUserId);
        Task<List<ActivityLogDto>?> GetLogsAsync(int taskId, int currentUserId);
    }
}
