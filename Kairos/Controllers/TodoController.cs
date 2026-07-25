using Kairos.Shared.DTOs;
using Kairos.Models;
using Kairos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Kairos.Controllers
{
    public static class TodoExtensions
    {
        public static TodoResponse FromEntity(this Models.TodoItem todoItem)
        {
            return new TodoResponse
            {
                ID = todoItem.ID,
                Title = todoItem.Title,
                Description = todoItem.Description,
                IsCompleted = todoItem.IsCompleted,
                CompletedTime = todoItem.CompletedTime,
                CreatedTime = todoItem.CreatedTime,
                ProjectID = todoItem.ProjectID
            };
        }
    }

    [Authorize]
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

        private string GetUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        }

        [HttpPost]
        public IActionResult Create(CreateTodoRequest request)
        {
            var project = _projectService.GetByID(request.ProjectID, GetUserId());
            if (project == null)
                return BadRequest("존재하지 않거나 접근할 수 없는 프로젝트입니다.");

            var todo = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
                ProjectID = request.ProjectID
            };
            var createdTodo = _todoService.Create(todo);
            var response = new CreateTodoResponse { ID = createdTodo.ID };

            return CreatedAtAction(nameof(GetByID), new { id = createdTodo.ID }, response);
        }

        [HttpGet]
        public IActionResult GetAll()
        {

            var todos = _todoService.GetByUser(GetUserId());
            var response = todos.Select(TodoExtensions.FromEntity).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetByID(int id)
        {
            var todo = _todoService.GetByID(id, GetUserId());
            if (todo == null)
                return NotFound();

            return Ok(TodoExtensions.FromEntity(todo));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateTodoRequest request)
        {
            var todo = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
            };

            var updatedTodo = _todoService.Update(id, todo, GetUserId());
            if (!updatedTodo)
                return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/completed")]
        public IActionResult SetCompleted(int id, SetCompletedRequest request)
        {
            var updatedTodo = _todoService.SetCompleted(id, request.IsCompleted,GetUserId());
            if (!updatedTodo)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var deleted = _todoService.Delete(id, GetUserId());
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
