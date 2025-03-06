using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;
using PersonalFinanceTracker.AzureApi.Services;

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
            _logger.LogInformation("UserController constructor called");
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<User>> GetUser(string username)
        {
            _logger.LogInformation("GET request for username: {Username}", username);
            var user = await _cosmosDbService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return NotFound();
            }
            _logger.LogInformation("User retrieved: {UserId}", user.UserId);
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            _logger.LogInformation("POST request to create user: {UserId}", user.UserId);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _cosmosDbService.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { username = user.Username }, user);
        }
    }
}