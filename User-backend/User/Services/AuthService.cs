using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User.Data;
using User.Entities;
using User.DTOs;

namespace User.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(UserDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public class RegisterResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public EntitieUser? User { get; set; }
        }


        /* Registration */
        public async Task<RegisterResult> CreateUserAsync(RegisterDto request, string role)
        {
            var allowedRoles = Enum.GetNames(typeof(UserRole));

            if (string.IsNullOrWhiteSpace(role))
            {
                return new RegisterResult
                {
                    Success = false,
                    ErrorMessage = "Role is required."
                };
            }

            var matchedRole = allowedRoles
                .FirstOrDefault(r => r.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase));

            if (matchedRole == null)
            {
                return new RegisterResult
                {
                    Success = false,
                    ErrorMessage = $"Invalid role. Allowed roles: {string.Join(", ", allowedRoles)}"
                };
            }

            role = matchedRole;


            var userExists = await _context.Users.AnyAsync(u => u.UserName == request.UserName);
            if (userExists)
            {
                return new RegisterResult
                {
                    Success = false,
                    ErrorMessage = "UserName already Exists"
                };
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
            {
                return new RegisterResult
                {
                    Success = false,
                    ErrorMessage = "Email already Exists"
                };
            }

            var user = new EntitieUser();
            var hassedPassword = new PasswordHasher<EntitieUser>()
                .HashPassword(user, request.Password);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UserName = request.UserName;
            user.Email = request.Email;
            user.PhoneNo = request.PhoneNo;
            user.PasswordHash = hassedPassword;
            user.Role = role;   // Role now comes from controller

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new RegisterResult
            {
                Success = true,
                User = user
            };
        }

        /* AddUser */
        public async Task<RegisterResult> CreateUserAsync(AddUserDto request)
        {
            return await CreateUserAsync
                (
                    new RegisterDto
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        UserName = request.UserName,
                        Email = request.Email,
                        PhoneNo = request.PhoneNo,
                        Password = request.Password //plain password
                    },
                    request.Role
                );
        }

        /* Login Logic */
        public async Task<TokenResponceDto?> LoginAsync(LoginDto request)
        {

            /* (1) */
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserNameOrEmail || u.Email == request.UserNameOrEmail);
            if (user == null)
            {
                return null;
            }
            if (new PasswordHasher<EntitieUser>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponce(user);
        }

        private async Task<TokenResponceDto> CreateTokenResponce(EntitieUser user)
        {
            /* redirect From the LoginAsync (2) */
            return new TokenResponceDto
            {
                AccessToken = CreateToken(user), /* Go to the CreateToken() */
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user), /* Go to the function */
                Role = user.Role
            };
        }
        private string CreateToken(EntitieUser user)
        {
            /* redirect from the CreateTokenResponce() (3) */
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDscriptor = new JwtSecurityToken(
                    issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                    audience: _configuration.GetValue<string>("AppSettings:Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDscriptor);

            /* redirect to the CreateTokenResponce() */
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(EntitieUser user)
        {

            /* redirect from the CreateTokenResponce() (4) */
            var refreshToken = GenerateRefreshToken(); //go to the funciton
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
            return refreshToken;

            /* redirect to the CreateTokenResponce */
        }

        private string GenerateRefreshToken()
        {
            /* redirect from the GenerateAndSaveRefreshTokenAsync() (5) */
            var rendomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rendomNumber);
            return Convert.ToBase64String(rendomNumber);

            /* redirect to the GenerateAndSaveRefreshTokenAsync() */
        }

        public async Task<TokenResponceDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            /* refresh token (1) */
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken); //go to the function
            if (user is null)
            {
                return null;
            }
            return await CreateTokenResponce(user);
        }

        private async Task<EntitieUser?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            /* redirect from the RefreshTokenAsync() */
            var user = await _context.Users.FindAsync(userId);
            if (user is null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryTime == null ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;

            /* return to the RefreshTokenAsync() */
        }


        public async Task<List<UserDisplayDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDisplayDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNo = u.PhoneNo,
                    Role = u.Role
                })
                .ToListAsync();
        }

        public async Task<TokenResponceDto> FacebookLoginAsync(string name, string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new EntitieUser
                {
                    FirstName = name,
                    LastName = "",
                    UserName = !string.IsNullOrEmpty(email)
                        ? email.Split('@')[0]
                        : name.Replace(" ", "").ToLower(),

                    Email = string.IsNullOrWhiteSpace(email)
                        ? $"{name.Replace(" ", "").ToLower()}@facebook.local"
                        : email,
                    PhoneNo = "",
                    PasswordHash = "",
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return await CreateTokenResponce(user);
        }

        public async Task<TokenResponceDto> GitHubLoginAsync(
            string name,
            string userName,
            string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new EntitieUser
                {
                    FirstName = name,
                    LastName = "",
                    UserName = userName + "_github",
                    Email = email,
                    PhoneNo = "",
                    PasswordHash = "",
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return await CreateTokenResponce(user);
        }
    }
}
