using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.API.Models
{
    public class ShortenedUrl
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ShortCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int ClickCount { get; set; } = 0;

        // Foreign Key
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<UrlClick> UrlClicks { get; set; } = new List<UrlClick>();
    }
}