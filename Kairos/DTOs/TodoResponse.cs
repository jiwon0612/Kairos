namespace Kairos.DTOs
{
    public class TodoResponse
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CompletedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ProjectID { get; set; }

        public static TodoResponse FromEntity(Models.TodoItem todoItem)
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
}
