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

        public StatementController(ICloudDbService cloudDbService)
        {
            _cosmosDbService = cloudDbService;
        }

        [HttpGet("{statementId}/{userId}")]
        public async Task<ActionResult<Statement>> GetStatement(string statementId, string userId)
        {
            var statement = await _cosmosDbService.GetStatementsByUserIdAsync(userId);
            if (statement == null) return NotFound();
            return Ok(statement);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStatement([FromBody] Statement statement)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _cosmosDbService.SaveStatementAsync(statement);
            return CreatedAtAction(nameof(GetStatement), new { statementId = statement.StatementId, userId = statement.UserId }, statement);
        }
    }
}
