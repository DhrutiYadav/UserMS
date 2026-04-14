using Humanizer.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json;
using User.CustomAttribute;
using User.DTOs;
using User.Entities;
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
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration? configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
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

        [HttpPost("facebook")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginDto model)
        {
            try
            {
                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(
                    $"https://graph.facebook.com/me?fields=id,name,email&access_token={model.AccessToken}"
                );

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Invalid Facebook token");

                var content = await response.Content.ReadAsStringAsync();

                var facebookUser = JsonSerializer.Deserialize<FacebookUserDto>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // IMPORTANT FIX START
                if (facebookUser == null)
                    return BadRequest("Facebook user data not found");

                if (string.IsNullOrWhiteSpace(facebookUser.Email))
                {
                    facebookUser.Email =
                        $"{facebookUser.Name.Replace(" ", "").ToLower()}@facebook.local";
                }
                var tokenResponse = await _authService.FacebookLoginAsync(
                    facebookUser.Name,
                    facebookUser.Email
                );

                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("github")]
        public async Task<IActionResult> GitHubLogin([FromBody] GitHubLoginDto model)
        {
            try
            {
                using var httpClient = new HttpClient();

                // STEP 1: Exchange code for token
                var tokenRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://github.com/login/oauth/access_token"
                );

                tokenRequest.Headers.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                );

                tokenRequest.Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                { "client_id", _configuration["GitHub:ClientId"] },
                { "client_secret", _configuration["GitHub:ClientSecret"] },
                { "code", model.Code }
                    }
                );

                var tokenResponse = await httpClient.SendAsync(tokenRequest);
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

                Console.WriteLine("TOKEN STATUS: " + tokenResponse.StatusCode);
                Console.WriteLine("TOKEN JSON: " + tokenJson);
                var tokenData = JsonSerializer.Deserialize<GitHubTokenResponse>(tokenJson);

                if (string.IsNullOrEmpty(tokenData?.AccessToken))
                    return BadRequest(tokenJson);

                // STEP 2: Get GitHub user
                var userRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    "https://api.github.com/user"
                );

                userRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

                userRequest.Headers.UserAgent.ParseAdd("MyApp");

                var userResponse = await httpClient.SendAsync(userRequest);

                var userJson = await userResponse.Content.ReadAsStringAsync();

                Console.WriteLine("USER STATUS: " + userResponse.StatusCode);
                Console.WriteLine("USER JSON: " + userJson);
                var githubUser = JsonSerializer.Deserialize<GitHubUserDto>(
                    userJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (githubUser == null)
                    return BadRequest("GitHub user data not found");

                var email = githubUser.Email;

                if (string.IsNullOrWhiteSpace(email))
                {
                    try
                    {
                        var emailRequest = new HttpRequestMessage(
                            HttpMethod.Get,
                            "https://api.github.com/user/emails"
                        );

                        emailRequest.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

                        emailRequest.Headers.UserAgent.ParseAdd("MyApp");

                        var emailResponse = await httpClient.SendAsync(emailRequest);

                        var emailJson = await emailResponse.Content.ReadAsStringAsync();

                        Console.WriteLine("EMAIL API STATUS: " + emailResponse.StatusCode);
                        Console.WriteLine("EMAIL API RESPONSE: " + emailJson);

                        if (emailResponse.IsSuccessStatusCode)
                        {
                            var emails = JsonSerializer.Deserialize<List<GitHubEmailDto>>(
                                emailJson,
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                            email = emails?
                                .FirstOrDefault(e => e.Primary)?.Email
                                ?? $"{githubUser.Login}@github.local";
                        }
                        else
                        {
                            email = $"{githubUser.Login}@github.local";
                        }
                    }
                    catch
                    {
                        email = $"{githubUser.Login}@github.local";
                    }
                }

                var jwtResponse = await _authService.GitHubLoginAsync(
                    githubUser.Name ?? githubUser.Login,
                    githubUser.Login,
                    email
                );

                return Ok(jwtResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}