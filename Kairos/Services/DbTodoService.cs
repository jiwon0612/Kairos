using Kairos.Data;
using Kairos.Models;

namespace Kairos.Services
{
    public class DbTodoService : ITodoService
    {
        private readonly KairosDbContext _context;

        public DbTodoService(KairosDbContext context)
        {
            _context = context;
        }

        public TodoItem Create(TodoItem todoItem)
        {
            todoItem.CreatedTime = DateTime.UtcNow;
            _context.Todos.Add(todoItem);
            _context.SaveChanges();
            return todoItem;
        }

        public bool Delete(int id, string userId)
        {
            var data = _context.Todos.FirstOrDefault(t => t.ID == id && t.Project!.UserID == userId);
            if (data == null)
                return false;

            _context.Todos.Remove(data);
            _context.SaveChanges();
            return true;
        }

        public List<TodoItem> GetByUser(string userId)
        {
            return _context.Todos.Where(p => p.Project!.UserID == userId).ToList();
        }

        public TodoItem? GetByID(int id, string userId)
        {
            return _context.Todos.FirstOrDefault(t => t.ID == id && t.Project!.UserID == userId);
        }

        public bool SetCompleted(int id, bool isCompleted, string userId)
        {
            var data = _context.Todos.FirstOrDefault(t => t.ID == id && t.Project!.UserID == userId);
            if (data == null)
                return false;
            data.IsCompleted = isCompleted;
            if (isCompleted)
            {
                data.CompletedTime = DateTime.UtcNow;
            }
            else
            {
                data.CompletedTime = default;
            }
            _context.SaveChanges();
            return true;
        }

        public bool Update(int id, TodoItem todoItem, string userId)
        {
            var data = _context.Todos.FirstOrDefault(t => t.ID == id && t.Project!.UserID == userId);
            if (data == null)
                return false;

            data.Title = todoItem.Title;
            data.Description = todoItem.Description;
            data.Priority = todoItem.Priority;
            data.DueDate = todoItem.DueDate;
            data.HasDueTime = todoItem.HasDueTime;
            _context.SaveChanges();
            return true;
        }

        public List<TodoItem> GetTodayTodos(string userId)
        {
            return _context.Todos
                .Where(t => t.Project!.UserID == userId
                && t.Project.IsToday
                && !t.IsCompleted)
                .OrderByDescending(t => t.Priority)
                .ToList();
        }
    }
}
