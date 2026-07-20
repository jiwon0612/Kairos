using Microsoft.EntityFrameworkCore;

namespace Kairos.Data
{
    public class KairosDbContext : DbContext
    {
        public KairosDbContext(DbContextOptions<KairosDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Project> Projects { get; set; } = null!;
        public DbSet<Models.TodoItem> Todos { get; set; } = null!;
    }
}
