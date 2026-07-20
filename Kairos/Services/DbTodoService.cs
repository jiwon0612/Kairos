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

        public bool Delete(int id)
        {
            var data = _context.Todos.FirstOrDefault(t => t.ID == id);
            if (data == null)
                return false;

            _context.Todos.Remove(data);
            _context.SaveChanges();
            return true;
        }

        public List<TodoItem> GetAll()
        {
            return _context.Todos.ToList();
        }

        public TodoItem? GetByID(int id)
        {
            return _context.Todos.FirstOrDefault(t => t.ID == id);
        }

        public bool SetCompleted(int id, bool isCompleted)
        {
            var data = _context.Todos.FirstOrDefault(t => t.ID == id);
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

        public bool Update(int id, TodoItem todoItem)
        {
            var data = _context.Todos.FirstOrDefault(t => t.ID == id);
            if (data == null)
                return false;

            data.Title = todoItem.Title;
            data.Description = todoItem.Description;
            _context.SaveChanges();
            return true;
        }
    }
}
