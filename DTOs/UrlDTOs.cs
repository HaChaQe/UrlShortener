using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.DTOs
{
    public class CreateUrlDto
    {
        [Required]
        [Url]
        public string OriginalUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(20)]
        public string? CustomCode { get; set; }
    }

    public class UrlResponseDto
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class UrlStatsDto
    {
        public int Id { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int TotalClicks { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DailyClickDto> DailyClicks { get; set; } = new List<DailyClickDto>();
        public List<CountryClickDto> CountryStats { get; set; } = new List<CountryClickDto>();
    }

    public class DailyClickDto
    {
        public DateTime Date { get; set; }
        public int Clicks { get; set; }
    }

    public class CountryClickDto
    {
        public string Country { get; set; } = string.Empty;
        public int Clicks { get; set; }
    }

    public class UpdateUrlDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool? IsActive { get; set; }
    }

    public class PaginatedResultDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}