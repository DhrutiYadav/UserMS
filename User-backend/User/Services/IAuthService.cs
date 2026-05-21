using User.Entities;
using User.DTOs;

namespace User.Services
{
    public interface IAuthService
    {
        Task<RegisterResultDto> CreateUserAsync(RegisterDto request, string role);
        Task<RegisterResultDto> CreateUserAsync(AddUserDto request);

        Task<TokenResponceDto?> LoginAsync(LoginDto request);
        Task<TokenResponceDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<List<UserDisplayDto>> GetAllUsersAsync();
        Task<TokenResponceDto> FacebookLoginAsync(string name, string email);
        Task<TokenResponceDto> GitHubLoginAsync(string name, string userName, string email);
    }
}
