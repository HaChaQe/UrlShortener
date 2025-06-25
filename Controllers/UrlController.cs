using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlShortener.API.DTOs;
using UrlShortener.API.Services;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UrlController : ControllerBase
    {
        private readonly IUrlShortenerService _urlService;
        private readonly ILogger<UrlController> _logger;

        public UrlController(IUrlShortenerService urlService, ILogger<UrlController> logger)
        {
            _urlService = urlService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShortUrl([FromBody] CreateUrlDto createUrlDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Kullanýcý kimliði bulunamadý" });
                }

                var result = await _urlService.CreateShortUrlAsync(createUrlDto, userId);

                return CreatedAtAction(nameof(GetUrl), new { id = result.Id },
                    new { message = "URL baþarýyla kýsaltýldý", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL oluþturma hatasý");
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUrl(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Kullanýcý kimliði bulunamadý" });
                }

                var result = await _urlService.GetUrlDetailsAsync(id, userId);

                if (result == null)
                {
                    return NotFound(new { message = "URL bulunamadý" });
                }

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL getirme hatasý: {Id}", id);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUrls(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Kullanýcý kimliði bulunamadý" });
                }

                var result = await _urlService.GetUserUrlsAsync(userId, page, pageSize, search);

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL listesi getirme hatasý");
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpGet("{id:int}/stats")]
        public async Task<IActionResult> GetUrlStats(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Kullanýcý kimliði bulunamadý" });
                }

                var result = await _urlService.GetUrlStatsAsync(id, userId);

                if (result == null)
                {
                    return NotFound(new { message = "URL bulunamadý" });
                }

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL istatistik hatasý: {Id}", id);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUrl(int id, [FromBody] UpdateUrlDto updateUrlDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Kullanýcý kimliði bulunamadý" });
                }

                var result = await _urlService.UpdateUrlAsync(id, updateUrlDto, userId);

                if (!result)
                {
                    return NotFound(new { message = "URL bulunamadý" });
                }

                return Ok(new { message = "URL baþarýyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL güncelleme hatasý: {Id}", id);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUrl(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Kullanýcý kimliði bulunamadý" });
                }

                var result = await _urlService.DeleteUrlAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { message = "URL bulunamadý" });
                }

                return Ok(new { message = "URL baþarýyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "URL silme hatasý: {Id}", id);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }
    }
}