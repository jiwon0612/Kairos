using Kairos.Models;

namespace Kairos.Services
{
    public class InMemoryProjectService// : IProjectService
    {
        private List<Project> _projects = new List<Project>();
        private int _nextId = 1;

        public Project Create(Project project)
        {
            project.ID = _nextId++;
            project.CreatedTime = DateTime.UtcNow;
            _projects.Add(project);
            return project;
        }

        public bool Update(int id, Project project)
        {
            var data = _projects.FirstOrDefault(p => p.ID == id);
            if (data == null)
                return false;

            data.Name = project.Name;
            data.Description = project.Description;
            return true;
        }

        public bool Delete(int id)
        {
            var data = _projects.FirstOrDefault(p => p.ID == id);
            if (data == null)
                return false;

            _projects.Remove(data);
            return true;
        }

        public List<Project> GetAll()
        {
            return _projects;
        }

        public Project? GetByID(int id)
        {
            return _projects.FirstOrDefault(p => p.ID == id);
        }
    }
}
