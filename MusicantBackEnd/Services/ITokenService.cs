using MusicantBackEnd.Models;

namespace MusicantBackEnd.Services
{
    public interface ITokenService
    {
        Task<string> GenerateToken(AppUser user);
    }
}