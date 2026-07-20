using Kairos.Models;

namespace Kairos.Services
{
    public class InMemoryTodoService : ITodoService
    {
        private List<TodoItem> _todoItems = new List<TodoItem>();
        private int _nextId = 1;

        public TodoItem Create(TodoItem todoItem)
        {
            todoItem.ID = _nextId++;
            todoItem.CreatedTime = DateTime.Now;
            _todoItems.Add(todoItem);
            return todoItem;
        }

        public bool Delete(int id)
        {
            var data = _todoItems.FirstOrDefault(t => t.ID == id);
            if (data == null)
                return false;

            _todoItems.Remove(data);
            return true;
        }

        public List<TodoItem> GetAll()
        {
            return _todoItems;
        }

        public TodoItem? GetByID(int id)
        {
            return _todoItems.FirstOrDefault(t => t.ID == id);
        }

        public bool Update(int id, TodoItem todoItem)
        {
            var data = _todoItems.FirstOrDefault(t => t.ID == id);
            if (data == null)
                return false;
            
            data.Title = todoItem.Title;
            data.Description = todoItem.Description;

            return true;
        }

        public bool SetCompleted(int id, bool isCompleted)
        {
            var data = _todoItems.FirstOrDefault(t => t.ID == id);
            if (data == null)
                return false;
            data.IsCompleted = isCompleted;
            if (isCompleted)
            {
                data.CompletedTime = DateTime.Now;
            }
            else
            {
                data.CompletedTime = default;
            }
            return true;
        }
    }
}
