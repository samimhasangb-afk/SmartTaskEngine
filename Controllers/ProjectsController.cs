using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskEngine.DTOs.Projects;
using SmartTaskEngine.Helpers;
using SmartTaskEngine.Services;

namespace SmartTaskEngine.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projects;

        public ProjectsController(IProjectService projects)
        {
            _projects = projects;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects() =>
            Ok(await _projects.GetProjectsAsync(CurrentUser.Id(User)));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _projects.GetProjectAsync(id, CurrentUser.Id(User));
            return project == null ? NotFound() : Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            var project = await _projects.CreateAsync(dto, CurrentUser.Id(User));
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
        {
            var project = await _projects.UpdateAsync(id, dto, CurrentUser.Id(User));
            return project == null ? NotFound() : Ok(project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _projects.DeleteAsync(id, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, AddProjectMemberDto dto)
        {
            var project = await _projects.AddMemberAsync(id, dto.UserId, CurrentUser.Id(User));
            return project == null ? NotFound(new { message = "Project or user not found." }) : Ok(project);
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            var ok = await _projects.RemoveMemberAsync(id, userId, CurrentUser.Id(User));
            return ok ? NoContent() : NotFound();
        }
    }
}
