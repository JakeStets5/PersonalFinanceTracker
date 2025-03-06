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
            // Call Cosmos DB to authenticate—returns User if valid, null if not
            var user = await _cosmosDbService.AuthenticateUserAsync(request.Username, request.Password);

            // Check auth result. Null means bad creds, triggers 401 response.
            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Success—return 200 OK with User object as JSON.
            return Ok(user);
        }
    }
}