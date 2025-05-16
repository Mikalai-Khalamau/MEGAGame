using Microsoft.EntityFrameworkCore;
using MEGAGame.Core.Models;

namespace MEGAGame.Core.Data
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Question> Questions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=game.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>().HasKey(p => p.Id);
            modelBuilder.Entity<Theme>().HasKey(t => t.Id);
            modelBuilder.Entity<Question>().HasKey(q => q.Id);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Theme)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.ThemeId);
        }
    }
}