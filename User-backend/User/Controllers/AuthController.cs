using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using User.CustomAttribute;
using User.Entities;
using User.DTOs;
using User.Services;
using static User.Services.AuthService;

namespace User.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterDto request)
        {
            try
            {
                var result = await _authService.CreateUserAsync(request, "Manager");

                if (!result.Success)
                {
                    _logger.LogWarning("the field Not fill or UserName Taken: {UserName}", request.UserName);
                    return Conflict(new { message = result.ErrorMessage }); // 🔥 409 here
                }
                _logger.LogInformation("Registration successfully the user is Manager UseName: {UserName}", request.UserName);
                return Ok(result.User);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured during registration for user: {UserName}", request.UserName);
                return StatusCode(500, "Internal Server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addUser")]
        public async Task<ActionResult<RegisterResult>> AddUser([FromBody] AddUserDto request)
        {
            try
            {
                _logger.LogInformation("Admin Add the User: {UserName}", request.UserName);
                var result = await _authService.CreateUserAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("the field Not fill or UserName Taken: {UserName}", request.UserName);
                    return Conflict(new { message = result.ErrorMessage });
                }
                _logger.LogInformation("Registration successfully!!: {UserName}", request.UserName);
                return Ok(result.User);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ocured during add user {UserName}", request.UserName);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        //[ApiKey]
        public async Task<ActionResult> Login(LoginDto request)
        {
            try
            {
                _logger.LogInformation("login attempt for user: {UserName}", request.UserNameOrEmail);
                var result = await _authService.LoginAsync(request);
                if (result == null)
                {
                    _logger.LogWarning("Invalid username or Password: {UserName}", request.UserNameOrEmail);
                    return BadRequest("Invalid username or Password");
                }

                _logger.LogInformation("login successful for user: {UserName}", request.UserNameOrEmail);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured during login for: {UserNameOrEmail}", request.UserNameOrEmail);
                return StatusCode(500, "Internal Server error");
            }
        }


        [HttpPost("RefreshToken")]
        public async Task<ActionResult<TokenResponceDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);

                if (result is null || result.AccessToken is null || result.RefreshToken is null)
                {
                    _logger.LogInformation("Invalid refresh token");
                    return Unauthorized("Invalid Refresh Token");
                }
                _logger.LogInformation("Valid Refresh Token");
                return Ok(result);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error occurred while refreshing token");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("usersDisplay")]
        [ApiKey]
        public async Task<ActionResult<List<UserDisplayDto>>> GetUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
