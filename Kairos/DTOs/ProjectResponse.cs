namespace Kairos.DTOs
{
    public class ProjectResponse
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreateTime { get; set; }

        public static ProjectResponse FromEntity(Models.Project project)
        {
            return new ProjectResponse
            {
                ID = project.ID,
                Name = project.Name,
                Description = project.Description,
                CreateTime = project.CreatedTime
            };
        }
    }
}
