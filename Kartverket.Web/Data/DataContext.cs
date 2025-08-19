using Microsoft.EntityFrameworkCore;

namespace Kartverket.Web.Data
{

    public class DataContext : DbContext
    {
        public DbSet<TableClass> TableClasses { get; set; } = null!;
        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }

    }
}
