using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Interfaces;
using System.Net;

namespace PersonalFinanceTracker.AzureApi.Controllers
{
    [Route("api/[controller]")] // Route: /api/statements
    [ApiController]
    public class StatementController : ControllerBase
    {
        private readonly ICloudDbService _cosmosDbService; // Service for Cosmos DB operations
        private readonly ILogger<StatementController> _logger;

        public StatementController(ICloudDbService cloudDbService, ILogger<StatementController> logger)
        {
            _cosmosDbService = cloudDbService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves statements for a specified user, optionally filtered by date range.
        /// </summary>
        /// <param name="userId">The ID of the user whose statements are retrieved. Required.</param>
        /// <param name="startDate">Optional start date for filtering statements (inclusive). Format: ISO 8601 (e.g., 2025-02-06T00:00:00-07:00).</param>
        /// <param name="endDate">Optional end date for filtering statements (inclusive, end of day). Format: ISO 8601 (e.g., 2025-03-06T23:59:59-07:00).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the statements as JSON if found.
        /// <list type="bullet">
        ///   <item><description>200 OK: Returns <see cref="IEnumerable{Statement}"/> with statements.</description></item>
        ///   <item><description>400 Bad Request: If <paramref name="userId"/> is missing or empty.</description></item>
        ///   <item><description>404 Not Found: If no statements are found for the user.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">Thrown by the underlying service if the query fails, logged internally.</exception>
        /// <remarks>
        /// GET /api/statements?userId={userId}&startDate={startDate}&endDate={endDate}
        /// Uses Cosmos DB hierarchical partition key (/UserId, /StatementId). Logs results or warnings.
        /// </remarks>
        [HttpGet] // Route: /api/statements
        public async Task<IActionResult> GetStatements([FromQuery] string userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            // Validate input
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            // Fetch statements from Cosmos DB
            var statements = await _cosmosDbService.GetStatementsByUserIdAsync(userId, startDate, endDate);
            if (statements == null || !statements.Any())
            {
                _logger.LogWarning("No statements found for userId: {UserId}", userId);
                return NotFound();
            }

            return Ok(statements);
        }

        /// <summary>
        /// Creates a new user in Cosmos DB.
        /// </summary>
        /// <param name="user">The user data to create, provided in the request body. Required.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> reflecting the result of the creation.
        /// <list type="bullet">
        ///   <item><description>201 Created: Returns the new <see cref="User"/> as JSON.</description></item>
        ///   <item><description>400 Bad Request: If <paramref name="user"/> is null or invalid.</description></item>
        ///   <item><description>409 Conflict: If the user ID already exists.</description></item>
        ///   <item><description>500 Internal Server Error: If creation fails unexpectedly.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="CosmosException">Thrown if Cosmos DB operations fail, logged and returned as status codes.</exception>
        /// <remarks>
        /// POST /api/users
        /// Saves the user to the 'Users' container with partition key /UserId. Logs success or errors.
        /// </remarks>
        [HttpPost] // Route: /api/statements
        public async Task<IActionResult> SubmitStatement([FromBody] Common.Models.Statement statement)
        {
            // Validate input + presence of partition keys
            if (statement == null || string.IsNullOrEmpty(statement.UserId) || string.IsNullOrEmpty(statement.StatementId))
            {
                return BadRequest("Statement must include UserId and StatementId");
            }

            // Ensure Id matches StatementId + generate if missing
            if (string.IsNullOrEmpty(statement.Id))
            {
                statement.Id = string.IsNullOrEmpty(statement.StatementId)
                    ? Guid.NewGuid().ToString()
                    : statement.StatementId;
            }
            statement.Id = statement.StatementId; // Always sync Id to StatementId (Partition key shenanigans in cosmos)

            try
            {
                // Save statement to Cosmos DB
                await _cosmosDbService.SaveStatementAsync(statement);
                return Ok(statement);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogWarning("Statement conflict for Id: {Id}", statement.Id);
                return Conflict($"Statement with Id {statement.Id} already exists");
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to save statement for UserId: {UserId}, Status: {Status}, Substatus: {Substatus}",
                    statement.UserId, ex.StatusCode, ex.SubStatusCode);
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save statement for UserId: {UserId}", statement.UserId);
                return StatusCode(500, "Failed to save statement");
            }
        }
    }
}
