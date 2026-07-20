using Kairos.Data;
using Kairos.Models;

namespace Kairos.Services
{
    public class DbProjectService : IProjectService
    {
        private readonly KairosDbContext _context;

        public DbProjectService(KairosDbContext context)
        {
            _context = context;
        }

        public Project Create(Project project)
        {
            project.CreatedTime = DateTime.UtcNow;
            _context.Projects.Add(project);
            _context.SaveChanges();
            return project;
        }

        public bool Delete(int id)
        {
            var data = _context.Projects.FirstOrDefault(p => p.ID == id);
            if (data == null)
                return false;

            _context.Projects.Remove(data);
            _context.SaveChanges();
            return true;
        }

        public List<Project> GetAll()
        {
            return _context.Projects.ToList();
        }

        public Project? GetByID(int id)
        {
            return _context.Projects.FirstOrDefault(p => p.ID == id);
        }

        public bool Update(int id, Project project)
        {
            var data = _context.Projects.FirstOrDefault(p => p.ID == id);
            if (data == null)
                return false;

            data.Name = project.Name;
            data.Description = project.Description;
            _context.SaveChanges();
            return true;
        }
    }
}
