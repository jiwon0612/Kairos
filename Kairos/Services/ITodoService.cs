using Kairos.Models;

namespace Kairos.Services
{
    public interface ITodoService
    {
        List<TodoItem> GetByUser(string userId);
        TodoItem? GetByID(int id, string userId);
        TodoItem Create(TodoItem todoItem);
        bool Update(int id, TodoItem todoItem, string userId);
        bool SetCompleted(int id, bool isCompleted, string userId);
        bool Delete(int id, string userId);
    }
}
