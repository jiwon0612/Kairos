namespace Kairos.Models
{
    public class TodoItem
    {
        public int ID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CompletedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public int Priority { get; set; } = 0;
        public int ProjectID { get; set; }
        public Project? Project { get; set; }
    }
}
