using Microsoft.AspNetCore.Mvc;
using UrlShortener.API.Services;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("")]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlShortenerService _urlService;
        private readonly ILogger<RedirectController> _logger;

        public RedirectController(IUrlShortenerService urlService, ILogger<RedirectController> logger)
        {
            _urlService = urlService;
            _logger = logger;
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            try
            {
                if (string.IsNullOrEmpty(shortCode))
                {
                    return BadRequest(new { message = "Geçersiz kýsa kod" });
                }

                // Check if URL is expired
                var isExpired = await _urlService.IsUrlExpiredAsync(shortCode);
                if (isExpired)
                {
                    return NotFound(new { message = "Bu baðlantýnýn süresi dolmuþ veya mevcut deðil" });
                }

                var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);

                if (string.IsNullOrEmpty(originalUrl))
                {
                    return NotFound(new { message = "Baðlantý bulunamadý" });
                }

                // Record the click
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var referrer = HttpContext.Request.Headers["Referer"].ToString();

                _ = Task.Run(async () =>
                {
                    await _urlService.RecordClickAsync(shortCode, ipAddress, userAgent, referrer);
                });

                _logger.LogInformation("Yönlendirme: {ShortCode} -> {OriginalUrl}", shortCode, originalUrl);

                return Redirect(originalUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yönlendirme hatasý: {ShortCode}", shortCode);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }
    }
}