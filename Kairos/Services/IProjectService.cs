using Kairos.Models;

namespace Kairos.Services
{
    public interface IProjectService
    {
        List<Project> GetByUser(string userId);
        Project? GetByID(int id, string userId);
        Project Create(Project project);
        bool Update(int id, Project project, string userId);
        bool Delete(int id, string userId);
        bool SetToday(int id, string userId, bool isToday);
    }
}
