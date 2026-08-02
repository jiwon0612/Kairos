using Kairos.Data;
using Kairos.Models;
using Microsoft.EntityFrameworkCore;

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

        public bool Delete(int id, string userId)
        {
            var data = _context.Projects.FirstOrDefault(p => p.ID == id && p.UserID == userId);
            if (data == null)
                return false;

            _context.Projects.Remove(data);
            _context.SaveChanges();
            return true;
        }

        public List<Project> GetByUser(string userId)
        {
            return _context.Projects.Where(p => p.UserID == userId).Include(p => p.TodoItems).ToList();
        }

        public Project? GetByID(int id, string userId)
        {
            return _context.Projects.FirstOrDefault(p => p.ID == id && p.UserID == userId);
        }

        public bool Update(int id, Project project, string userId)
        {
            var data = _context.Projects.FirstOrDefault(p => p.ID == id && p.UserID == userId);
            if (data == null)
                return false;

            data.Name = project.Name;
            data.Description = project.Description;
            _context.SaveChanges();
            return true;
        }

        public bool SetToday(int id, string userId, bool isToday)
        {
            var data = _context.Projects.FirstOrDefault(p => p.ID == id && p.UserID == userId);
            if (data == null)
                return false;

            data.IsToday = isToday;
            _context.SaveChanges();
            return true;
        }
    }
}
