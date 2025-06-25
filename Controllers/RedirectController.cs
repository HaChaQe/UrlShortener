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
                    return BadRequest(new { message = "Ge�ersiz k�sa kod" });
                }

                // Check if URL is expired
                var isExpired = await _urlService.IsUrlExpiredAsync(shortCode);
                if (isExpired)
                {
                    return NotFound(new { message = "Bu ba�lant�n�n s�resi dolmu� veya mevcut de�il" });
                }

                var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);

                if (string.IsNullOrEmpty(originalUrl))
                {
                    return NotFound(new { message = "Ba�lant� bulunamad�" });
                }

                // Record the click
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var referrer = HttpContext.Request.Headers["Referer"].ToString();

                _ = Task.Run(async () =>
                {
                    await _urlService.RecordClickAsync(shortCode, ipAddress, userAgent, referrer);
                });

                _logger.LogInformation("Y�nlendirme: {ShortCode} -> {OriginalUrl}", shortCode, originalUrl);

                return Redirect(originalUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Y�nlendirme hatas�: {ShortCode}", shortCode);
                return StatusCode(500, new { message = "Sunucu hatas�" });
            }
        }
    }
}