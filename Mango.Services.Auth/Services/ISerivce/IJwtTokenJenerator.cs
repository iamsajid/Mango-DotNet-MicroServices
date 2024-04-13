using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Services.ISerivce
{
    public interface IJwtTokenJenerator
    {
        string GenerateJwtToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}