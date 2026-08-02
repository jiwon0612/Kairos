namespace Kairos.Shared.DTOs
{
    public class ProjectResponse
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsToday { get; set; }
    }
}
