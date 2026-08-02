namespace Kairos.Models
{
    public class Project
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public string? UserID { get; set; }
        public bool IsToday { get; set; }
        public List<TodoItem> TodoItems { get; set; } = new();
    }
}
