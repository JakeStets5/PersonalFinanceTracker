using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;
using System.Text.Json;
using System.Diagnostics;

namespace PersonalFinanceTracker.AzureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ICloudDbService _cosmosDbService;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, ICloudDbService cosmosDbService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cosmosDbService = cosmosDbService ?? throw new ArgumentNullException(nameof(cosmosDbService));
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<User>> GetUserAsync(string username)
        {
            var user = await _cosmosDbService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            _logger.LogInformation("POST request to create user: {UserId}", user.UserId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _cosmosDbService.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUserAsync), new { username = user.Username }, user);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            // Authenticate—get detailed result
            var authResult = await _cosmosDbService.AuthenticateUserAsync(request.Username, request.Password);

            // No user found—specific 404 response
            if (!authResult.UserFound)
            {
                _logger.LogWarning("No user found for username: {Username}", request.Username);
                return NotFound("Username not found");
            }

            // User found, wrong password—401 with context
            if (!authResult.PasswordMatch)
            {
                _logger.LogWarning("Password incorrect for username: {Username}", request.Username);
                return Unauthorized("Username or password incorrect");
            }

            // Success—return User with 200 OK.
            _logger.LogInformation("Sign-in successful for user: {UserId}", authResult.User.UserId);
            return Ok(authResult.User);
        }
    }
}