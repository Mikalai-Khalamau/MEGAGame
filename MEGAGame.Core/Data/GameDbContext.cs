using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MEGAGame.Core.Models;

namespace MEGAGame.Core.Data
{
    public class GameDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<QuestionPack> QuestionPacks { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<PlayedPack> PlayedPacks { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<PlayerAchievement> PlayerAchievements { get; set; }

        public GameDbContext() { }

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=localhost;Database=megagame_db;User=Nikolay;Password=7b2bru43.2d3we;",
                                       new MySqlServerVersion(new Version(8, 4, 0)))
                              .LogTo(Console.WriteLine, LogLevel.Information)
                              .EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Theme)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.ThemeId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Pack)
                .WithMany(p => p.Questions)
                .HasForeignKey(q => q.PackId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.CreatedByPlayer)
                .WithMany()
                .HasForeignKey(q => q.CreatedBy);

            modelBuilder.Entity<PlayedPack>()
                .HasOne(pp => pp.Player)
                .WithMany()
                .HasForeignKey(pp => pp.PlayerId);

            modelBuilder.Entity<PlayedPack>()
                .HasOne(pp => pp.Pack)
                .WithMany()
                .HasForeignKey(pp => pp.PackId);

            modelBuilder.Entity<Question>(e =>
            {
                e.Property(q => q.Answer2).IsRequired(false);
                e.Property(q => q.Answer3).IsRequired(false);
            });

            modelBuilder.Entity<PlayerAchievement>()
                .HasOne(pa => pa.Player)
                .WithMany()
                .HasForeignKey(pa => pa.PlayerId);

            modelBuilder.Entity<PlayerAchievement>()
                .HasOne(pa => pa.Achievement)
                .WithMany()
                .HasForeignKey(pa => pa.AchievementId);
        }
    }
}