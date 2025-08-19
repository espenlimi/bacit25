using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Kartverket.Web.Data
{
    public class TableClass
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

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
