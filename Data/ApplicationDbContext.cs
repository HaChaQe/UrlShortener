using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Models;

namespace UrlShortener.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
        public DbSet<UrlClick> UrlClicks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ShortenedUrl configuration
            builder.Entity<ShortenedUrl>(entity =>
            {
                entity.HasIndex(e => e.ShortCode).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ShortenedUrls)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UrlClick configuration
            builder.Entity<UrlClick>(entity =>
            {
                entity.HasIndex(e => e.ShortenedUrlId);
                entity.HasIndex(e => e.ClickedAt);

                entity.HasOne(d => d.ShortenedUrl)
                    .WithMany(p => p.UrlClicks)
                    .HasForeignKey(d => d.ShortenedUrlId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}