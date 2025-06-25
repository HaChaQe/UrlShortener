using UrlShortener.API.DTOs;

namespace UrlShortener.API.Services
{
    public interface IUrlShortenerService
    {
        Task<UrlResponseDto> CreateShortUrlAsync(CreateUrlDto createUrlDto, string userId);
        Task<string?> GetOriginalUrlAsync(string shortCode);
        Task<UrlResponseDto?> GetUrlDetailsAsync(int id, string userId);
        Task<PaginatedResultDto<UrlResponseDto>> GetUserUrlsAsync(string userId, int page, int pageSize, string? search = null);
        Task<UrlStatsDto?> GetUrlStatsAsync(int id, string userId);
        Task<bool> UpdateUrlAsync(int id, UpdateUrlDto updateUrlDto, string userId);
        Task<bool> DeleteUrlAsync(int id, string userId);
        Task RecordClickAsync(string shortCode, string? ipAddress, string? userAgent, string? referrer);
        Task<bool> IsUrlExpiredAsync(string shortCode);
    }
}