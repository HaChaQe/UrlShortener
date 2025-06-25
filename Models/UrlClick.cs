using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.API.Models
{
    public class UrlClick
    {
        [Key]
        public int Id { get; set; }

        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(200)]
        public string? Referrer { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        // Foreign Key
        public int ShortenedUrlId { get; set; }

        // Navigation Property
        [ForeignKey("ShortenedUrlId")]
        public virtual ShortenedUrl ShortenedUrl { get; set; } = null!;
    }
}