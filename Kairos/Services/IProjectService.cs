using Kairos.Models;

namespace Kairos.Services
{
    public interface IProjectService
    {
        List<Project> GetAll();
        Project? GetByID(int id);
        Project Create(Project project);
        bool Update(int id, Project project);
        bool Delete(int id);
    }
}
