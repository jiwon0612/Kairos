using Kairos.DTOs;
using Kairos.Models;
using Kairos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.Controllers
{
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
            };

            var createdProject = _projectService.Create(project);
            return CreatedAtAction(nameof(GetByID), new { id = createdProject.ID }, createdProject);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var projects = _projectService.GetAll();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public IActionResult GetByID(int id)
        {
            var project = _projectService.GetByID(id);
            if (project == null)
                return NotFound();
            return Ok(project);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, CreateProjectRequest request)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
            };

            var success = _projectService.Update(id, project);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _projectService.Delete(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
