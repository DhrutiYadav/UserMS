using User.Entities;
using User.DTOs;
using static User.Services.AuthService;

namespace User.Services
{
    public interface IAuthService
    {
        Task<RegisterResult> CreateUserAsync(RegisterDto request, string role);
        Task<AuthService.RegisterResult> CreateUserAsync(AddUserDto request);

        Task<TokenResponceDto?> LoginAsync(LoginDto request);
        Task<TokenResponceDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<List<UserDisplayDto>> GetAllUsersAsync();
        Task<TokenResponceDto> FacebookLoginAsync(string name, string email);
    }
}
