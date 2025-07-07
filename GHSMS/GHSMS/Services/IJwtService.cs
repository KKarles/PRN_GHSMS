using Repository.Models;

namespace GHSMS.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user, List<string> roles);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
        List<string> GetRolesFromToken(string token);
    }
}