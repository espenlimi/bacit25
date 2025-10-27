
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Web.Data
{

    public class DataContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<TableClass> TableClasses { get; set; } = null!;
        public DbSet<ObstacleData> ObstacleDatas { get; set; } = null!;

        public DataContext()
        {
        }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<TableClass>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
            modelBuilder.Entity<ObstacleData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ObstacleName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ObstacleHeight).IsRequired();
                entity.Property(e => e.ObstacleDescription).HasMaxLength(1000);
            });
            base.OnModelCreating(modelBuilder);
        }

    }
}
