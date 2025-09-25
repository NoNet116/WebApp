using DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleTags> ArticleTags { get; set; }

        public AppDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext> options) : base(options)
        {
            if (!Database.GetService<IRelationalDatabaseCreator>().Exists())
            {
                Database.EnsureCreated();
            }
            else
            {
                Database.Migrate();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                // UserName
                e.Property(u => u.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                e.HasIndex(u => u.UserName)
                    .IsUnique()
                    .HasDatabaseName("IX_User_UserName");

                // Email
                e.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                e.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_User_Email");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValue(DateTime.UtcNow);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValue(DateTime.UtcNow);
            });

            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd(); // Автоинкремент

                // Заменяем GETDATE() на значение по умолчанию
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValue(DateTime.UtcNow);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValue(DateTime.UtcNow);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                // Заменяем GETDATE() на значение по умолчанию
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValue(DateTime.UtcNow);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValue(DateTime.UtcNow);
            });

            #region ArticleTag

            modelBuilder.Entity<ArticleTags>(entity =>
            {
                entity.ToTable("ArticleTags");
                entity.HasKey(at => new { at.ArticleId, at.TagId });

                entity.HasOne(at => at.Article)
                    .WithMany(a => a.ArticleTags)  // Связь только с одной коллекцией
                    .HasForeignKey(at => at.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(at => at.Tag)
                    .WithMany(t => t.ArticleTags)
                    .HasForeignKey(at => at.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion ArticleTag
        }
    }
}