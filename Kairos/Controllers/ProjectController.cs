using Kairos.Shared.DTOs;
using Kairos.Models;
using Kairos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Kairos.Controllers
{
    public static class ProjectExtensions
    {
        public static ProjectResponse FromEntity(this Project project)
        {
            return new ProjectResponse
            {
                ID = project.ID,
                Name = project.Name,
                Description = project.Description,
                CreateTime = project.CreatedTime,
                IsToday = project.IsToday
            };
        }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public IActionResult Create(CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                UserID = GetUserId()
            };

            var createdProject = _projectService.Create(project);
            var response = new CreateProjectResponse { ID = createdProject.ID };
            return CreatedAtAction(nameof(GetByID), new { id = createdProject.ID }, response);
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var projects = _projectService.GetByUser(GetUserId());
            var response = projects.Select(ProjectExtensions.FromEntity).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetByID(int id)
        {
            var project = _projectService.GetByID(id, GetUserId());
            if (project == null)
                return NotFound();

            return Ok(project.FromEntity());
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
            };

            var success = _projectService.Update(id, project, GetUserId());
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _projectService.Delete(id, GetUserId());
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/today")]
        public IActionResult SetToday(int id, SetTodayRequest request)
        {
            var success = _projectService.SetToday(id,GetUserId(),request.IsToday);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
