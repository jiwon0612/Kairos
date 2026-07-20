using Kairos.DTOs;
using Kairos.Models;
using Kairos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly IProjectService _projectService;

        public TodoController(ITodoService todoService, IProjectService projectService)
        {
            _todoService = todoService;
            _projectService = projectService;
        }

        [HttpPost]
        public IActionResult Create(CreateTodoRequest request)
        {
            var project = _projectService.GetByID(request.ProjectID);
            if (project == null)
                return BadRequest($"{request.ProjectID}번 프로젝트는 없다");

            var todo = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
                ProjectID = request.ProjectID
            };
            var createdTodo = _todoService.Create(todo);
            return CreatedAtAction(nameof(GetByID), new { id = createdTodo.ID }, createdTodo);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var todos = _todoService.GetAll();
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public IActionResult GetByID(int id)
        {
            var todo = _todoService.GetByID(id);
            if (todo == null)
                return NotFound();
            return Ok(todo);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateTodoRequest request)
        {
            var todo = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
            };

            var updatedTodo = _todoService.Update(id, todo);
            if (!updatedTodo)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/completed")]
        public IActionResult SetCompleted(int id, SetCompletedRequest request)
        {
            var updatedTodo = _todoService.SetCompleted(id, request.IsCompleted);
            if (!updatedTodo)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var deleted = _todoService.Delete(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
