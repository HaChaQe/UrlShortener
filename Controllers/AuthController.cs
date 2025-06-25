using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.API.DTOs;
using UrlShortener.API.Models;
using UrlShortener.API.Services;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanýlýyor" });
                }

                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FullName = registerDto.FullName,
                    EmailConfirmed = true // Demo için direkt aktif
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Kullanýcý oluþturulamadý", errors = result.Errors });
                }

                var token = _tokenService.GenerateToken(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                _logger.LogInformation("Yeni kullanýcý kaydý: {Email}", registerDto.Email);

                return Ok(new { message = "Kayýt baþarýlý", data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayýt hatasý: {Email}", registerDto.Email);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Geçersiz email veya þifre" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Geçersiz email veya þifre" });
                }

                if (!user.IsActive)
                {
                    return BadRequest(new { message = "Hesabýnýz devre dýþý býrakýlmýþ" });
                }

                var token = _tokenService.GenerateToken(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Expires = DateTime.UtcNow.AddHours(24)
                };

                _logger.LogInformation("Kullanýcý giriþi: {Email}", loginDto.Email);

                return Ok(new { message = "Giriþ baþarýlý", data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriþ hatasý: {Email}", loginDto.Email);
                return StatusCode(500, new { message = "Sunucu hatasý" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Çýkýþ baþarýlý" });
        }
    }
}