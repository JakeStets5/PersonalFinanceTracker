using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Common.Interfaces;
using PersonalFinanceTracker.Common.Models;

namespace PersonalFinanceTracker.AzureApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatementController : ControllerBase
    {
        private readonly ICloudDbService _cosmosDbService;
        private readonly ILogger<StatementController> _logger;

        public StatementController(ICloudDbService cloudDbService, ILogger<StatementController> logger)
        {
            _cosmosDbService = cloudDbService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatements([FromQuery] string userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            var statements = await _cosmosDbService.GetStatementsByUserIdAsync(userId, startDate, endDate);
            if (statements == null || !statements.Any())
            {
                _logger.LogWarning("No statements found for userId: {UserId}", userId);
                return NotFound();
            }

            _logger.LogInformation("Retrieved {Count} statements for userId: {UserId}", statements.Count(), userId);
            return Ok(statements);
        }


        [HttpPost]
        public async Task<IActionResult> SubmitStatement([FromBody] Common.Models.Statement statement)
        {
            if(statement == null || string.IsNullOrEmpty(statement.UserId) || string.IsNullOrEmpty(statement.StatementId))
            {
                return BadRequest("Statement must include UserId and StatementId");
            }

            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                await _cosmosDbService.SaveStatementAsync(statement);
                return Ok(statement);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to submit statement for userId: {UserId}", statement.UserId);
                return StatusCode(500, "Failed to submit statement");
            }
        }
    }
}
