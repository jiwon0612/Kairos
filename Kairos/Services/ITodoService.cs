using Kairos.Models;

namespace Kairos.Services
{
    public interface ITodoService
    {
        List<TodoItem> GetAll();
        TodoItem? GetByID(int id);
        TodoItem Create(TodoItem todoItem);
        bool Update(int id, TodoItem todoItem);
        bool SetCompleted(int id, bool isCompleted);
        bool Delete(int id);
    }
}
