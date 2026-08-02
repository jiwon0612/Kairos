namespace Kairos.Shared.DTOs
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

        public int Priority { get; set; }
    }
}
