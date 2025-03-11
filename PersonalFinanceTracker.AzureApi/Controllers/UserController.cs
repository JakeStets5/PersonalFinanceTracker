using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;

namespace PersonalFinanceTracker.AzureApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route: /api/users
    public class UserController : ControllerBase
    {
        private readonly ICloudDbService _cosmosDbService;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, ICloudDbService cosmosDbService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cosmosDbService = cosmosDbService ?? throw new ArgumentNullException(nameof(cosmosDbService));
        }

        /// <summary>
        /// Retrieves a user by their username from Cosmos DB.
        /// </summary>
        /// <param name="username">The username of the user to retrieve. Required.</param>
        /// <returns>
        /// An <see cref="ActionResult{User}"/> containing the user data if found.
        /// <list type="bullet">
        ///   <item><description>200 OK: Returns the <see cref="User"/> as JSON.</description></item>
        ///   <item><description>404 Not Found: If no user exists with the specified username.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">Thrown by the underlying service if the query fails, logged internally.</exception>
        /// <remarks>
        /// GET /api/users/{username}
        /// Queries the 'Users' container by username. Returns the user if found, otherwise a 404.
        /// </remarks>
        [HttpGet("{username}")] // Route: /api/users/{username}
        public async Task<ActionResult<User>> GetUserAsync(string username)
        {
            var user = await _cosmosDbService.GetUserByUsernameAsync(username);

            // Return 404 if user not found
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user in Cosmos DB.
        /// </summary>
        /// <param name="user">The user data to create, provided in the request body. Required.</param>
        /// <returns>
        /// An <see cref="ActionResult{User}"/> reflecting the creation result.
        /// <list type="bullet">
        ///   <item><description>201 Created: Returns the new <see cref="User"/> with a Location header to GET /api/users/{username}.</description></item>
        ///   <item><description>400 Bad Request: If <paramref name="user"/> is null or invalid per model validation.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">Thrown by the underlying service if saving fails, logged internally.</exception>
        /// <remarks>
        /// POST /api/users
        /// Adds the user to the 'Users' container with partition key /UserId. Logs the creation attempt.
        /// </remarks>
        [HttpPost] // Route: /api/users
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            // Validates client input
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Save user to Cosmos DB
            await _cosmosDbService.AddUserAsync(user);
            return Ok(user);
        }

        /// <summary>
        /// Authenticates a user and signs them in based on username and password.
        /// </summary>
        /// <param name="request">The sign-in request containing username and password, provided in the request body. Required.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> reflecting the authentication result.
        /// <list type="bullet">
        ///   <item><description>200 OK: Returns the authenticated <see cref="User"/> as JSON.</description></item>
        ///   <item><description>400 Bad Request: If <paramref name="request"/> is null or invalid.</description></item>
        ///   <item><description>401 Unauthorized: If the password is incorrect.</description></item>
        ///   <item><description>404 Not Found: If the username does not exist.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">Thrown by the underlying service if authentication fails unexpectedly, logged internally.</exception>
        /// <remarks>
        /// POST /api/users/signin
        /// Queries the 'Users' container to authenticate. Logs success, missing users, or incorrect passwords.
        /// </remarks>
        [HttpPost("signin")] // Route: /api/users/signin
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

            return Ok(authResult.User);
        }
    }
}