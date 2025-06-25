using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.DTOs;
using UrlShortener.API.Models;
using System.Text;

namespace UrlShortener.API.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UrlShortenerService> _logger;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public UrlShortenerService(ApplicationDbContext context, ILogger<UrlShortenerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UrlResponseDto> CreateShortUrlAsync(CreateUrlDto createUrlDto, string userId)
        {
            try
            {
                var shortCode = string.IsNullOrEmpty(createUrlDto.CustomCode)
                    ? await GenerateUniqueShortCodeAsync()
                    : createUrlDto.CustomCode;

                // Check if custom code already exists
                if (!string.IsNullOrEmpty(createUrlDto.CustomCode))
                {
                    var existingUrl = await _context.ShortenedUrls
                        .FirstOrDefaultAsync(u => u.ShortCode == createUrlDto.CustomCode);

                    if (existingUrl != null)
                    {
                        throw new InvalidOperationException("Bu kýsa kod zaten kullanýlýyor.");
                    }
                }

                var shortenedUrl = new ShortenedUrl
                {
                    OriginalUrl = createUrlDto.OriginalUrl,
                    ShortCode = shortCode,
                    Title = createUrlDto.Title,
                    Description = createUrlDto.Description,
                    ExpiresAt = createUrlDto.ExpiresAt,
                    UserId = userId
                };

                _context.ShortenedUrls.Add(shortenedUrl);
                await _context.SaveChangesAsync();

                _logger.LogInformation("URL kýsaltýldý: {ShortCode} -> {OriginalUrl}", shortCode, createUrlDto.OriginalUrl);

                return new UrlResponseDto
                {
                    Id = shortenedUrl.Id,
                    OriginalUrl = shortenedUrl.OriginalUrl,
                    ShortCode = shortenedUrl.ShortCode,
                    ShortUrl = $"https://localhost:7000/{shortenedUrl.ShortCode}",
                    Title = shortenedUrl.Title,
                    Description = shortenedUrl.Description,
                    CreatedAt = shortenedUrl.CreatedAt,
                    ExpiresAt = shortenedUrl.ExpiresAt,
                    ClickCount = shortenedUrl.ClickCount,
                    IsActive = shortenedUrl.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL kýsaltma hatasý: {Error}", ex.Message);
                throw;
            }
        }

        public async Task<string?> GetOriginalUrlAsync(string shortCode)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.ShortCode == shortCode && u.IsActive);

                if (url == null)
                    return null;

                // Check if expired
                if (url.ExpiresAt.HasValue && url.ExpiresAt.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Süresi dolmuþ URL eriþim denemesi: {ShortCode}", shortCode);
                    return null;
                }

                return url.OriginalUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL eriþim hatasý: {ShortCode}", shortCode);
                return null;
            }
        }

        public async Task<UrlResponseDto?> GetUrlDetailsAsync(int id, string userId)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);

                if (url == null)
                    return null;

                return new UrlResponseDto
                {
                    Id = url.Id,
                    OriginalUrl = url.OriginalUrl,
                    ShortCode = url.ShortCode,
                    ShortUrl = $"https://localhost:7000/{url.ShortCode}",
                    Title = url.Title,
                    Description = url.Description,
                    CreatedAt = url.CreatedAt,
                    ExpiresAt = url.ExpiresAt,
                    ClickCount = url.ClickCount,
                    IsActive = url.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL detay getirme hatasý: {Id}", id);
                return null;
            }
        }

        public async Task<PaginatedResultDto<UrlResponseDto>> GetUserUrlsAsync(string userId, int page, int pageSize, string? search = null)
        {
            try
            {
                var query = _context.ShortenedUrls
                    .Where(u => u.UserId == userId);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.OriginalUrl.Contains(search) ||
                                           (u.Title != null && u.Title.Contains(search)) ||
                                           u.ShortCode.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var urls = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UrlResponseDto
                    {
                        Id = u.Id,
                        OriginalUrl = u.OriginalUrl,
                        ShortCode = u.ShortCode,
                        ShortUrl = $"https://localhost:7000/{u.ShortCode}",
                        Title = u.Title,
                        Description = u.Description,
                        CreatedAt = u.CreatedAt,
                        ExpiresAt = u.ExpiresAt,
                        ClickCount = u.ClickCount,
                        IsActive = u.IsActive
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new PaginatedResultDto<UrlResponseDto>
                {
                    Data = urls,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanýcý URL'leri getirme hatasý: {UserId}", userId);
                throw;
            }
        }

        public async Task<UrlStatsDto?> GetUrlStatsAsync(int id, string userId)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .Include(u => u.UrlClicks)
                    .FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);

                if (url == null)
                    return null;

                var dailyClicks = url.UrlClicks
                    .GroupBy(c => c.ClickedAt.Date)
                    .Select(g => new DailyClickDto
                    {
                        Date = g.Key,
                        Clicks = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var countryStats = url.UrlClicks
                    .Where(c => !string.IsNullOrEmpty(c.Country))
                    .GroupBy(c => c.Country!)
                    .Select(g => new CountryClickDto
                    {
                        Country = g.Key,
                        Clicks = g.Count()
                    })
                    .OrderByDescending(c => c.Clicks)
                    .ToList();

                return new UrlStatsDto
                {
                    Id = url.Id,
                    ShortCode = url.ShortCode,
                    OriginalUrl = url.OriginalUrl,
                    TotalClicks = url.ClickCount,
                    CreatedAt = url.CreatedAt,
                    DailyClicks = dailyClicks,
                    CountryStats = countryStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL istatistik hatasý: {Id}", id);
                return null;
            }
        }

        public async Task<bool> UpdateUrlAsync(int id, UpdateUrlDto updateUrlDto, string userId)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);

                if (url == null)
                    return false;

                if (updateUrlDto.Title != null)
                    url.Title = updateUrlDto.Title;

                if (updateUrlDto.Description != null)
                    url.Description = updateUrlDto.Description;

                if (updateUrlDto.ExpiresAt.HasValue)
                    url.ExpiresAt = updateUrlDto.ExpiresAt;

                if (updateUrlDto.IsActive.HasValue)
                    url.IsActive = updateUrlDto.IsActive.Value;

                await _context.SaveChangesAsync();

                _logger.LogInformation("URL güncellendi: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL güncelleme hatasý: {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteUrlAsync(int id, string userId)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.Id == id && u.UserId == userId);

                if (url == null)
                    return false;

                _context.ShortenedUrls.Remove(url);
                await _context.SaveChangesAsync();

                _logger.LogInformation("URL silindi: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL silme hatasý: {Id}", id);
                return false;
            }
        }

        public async Task RecordClickAsync(string shortCode, string? ipAddress, string? userAgent, string? referrer)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

                if (url == null)
                    return;

                // Increment click count
                url.ClickCount++;

                // Record click details
                var click = new UrlClick
                {
                    ShortenedUrlId = url.Id,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Referrer = referrer,
                    Country = "Turkey", // Basit bir örnek, gerçekte IP'den ülke tespiti yapýlabilir
                    City = "Ýstanbul"
                };

                _context.UrlClicks.Add(click);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Týklama kaydedildi: {ShortCode}", shortCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Týklama kaydetme hatasý: {ShortCode}", shortCode);
            }
        }

        public async Task<bool> IsUrlExpiredAsync(string shortCode)
        {
            try
            {
                var url = await _context.ShortenedUrls
                    .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

                if (url == null)
                    return true;

                return url.ExpiresAt.HasValue && url.ExpiresAt.Value < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL süresi kontrol hatasý: {ShortCode}", shortCode);
                return true;
            }
        }

        private async Task<string> GenerateUniqueShortCodeAsync()
        {
            const int maxAttempts = 10;
            var random = new Random();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var code = GenerateRandomCode(6 + attempt); // Her denemede kod uzunluðunu artýr

                var exists = await _context.ShortenedUrls
                    .AnyAsync(u => u.ShortCode == code);

                if (!exists)
                    return code;
            }

            // Son çare olarak timestamp ekle
            return GenerateRandomCode(6) + DateTime.UtcNow.Ticks.ToString().Substring(0, 4);
        }

        private static string GenerateRandomCode(int length)
        {
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(Characters[random.Next(Characters.Length)]);
            }

            return result.ToString();
        }
    }
}