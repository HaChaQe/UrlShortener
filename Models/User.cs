using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ShortenedUrl> ShortenedUrls { get; set; } = new List<ShortenedUrl>();
    }
}