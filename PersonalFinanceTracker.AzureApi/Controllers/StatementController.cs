using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Interfaces;
using PersonalFinanceTracker.Common.Models;
using System.Net;
using System.Text.Json.Serialization;

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

        [HttpPost("raw")]
        public async Task<IActionResult> SubmitRawStatement([FromBody] JsonRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Json))
                    return BadRequest("JSON content is required");

                _logger.LogInformation("Raw request: {Json}", request.Json);
                await _cosmosDbService.SaveRawStatementAsync(request.Json);
                return Ok("Raw statement saved");
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Cosmos error with raw JSON: {Json}", request.Json);
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
        }

        public class JsonRequest
        {
            [JsonPropertyName("json")]
            public string Json { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> SubmitStatement([FromBody] Common.Models.Statement statement)
        {
            if(statement == null || string.IsNullOrEmpty(statement.UserId) || string.IsNullOrEmpty(statement.StatementId))
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
                await _cosmosDbService.SaveStatementAsync(statement);
                _logger.LogInformation("Statement saved for UserId: {UserId}, StatementId: {StatementId}",
                    statement.UserId, statement.StatementId);
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
