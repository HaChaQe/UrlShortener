using UrlShortener.API.Models;

namespace UrlShortener.API.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string? ValidateToken(string token);
    }
}