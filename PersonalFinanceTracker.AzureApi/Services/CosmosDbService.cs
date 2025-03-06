using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;
using Microsoft.Extensions.Logging; // For ILogger
using User = PersonalFinanceTracker.Common.Models.User;
using BCrypt.Net;
using System.Text.Json;
using System.Diagnostics;

namespace PersonalFinanceTracker.AzureApi.Services
{
    public class CosmosDbService : ICloudDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _userContainer;
        private readonly Container _statementContainer;
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(CosmosClient cosmosClient, IConfiguration config, ILogger<CosmosDbService> logger)
        {
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var cosmosConfig = config.GetSection("CosmosDb") ?? throw new ArgumentNullException(nameof(config));
            var dbName = cosmosConfig["DatabaseName"];
            var database = _cosmosClient.GetDatabase(dbName);
            _userContainer = database.GetContainer(cosmosConfig["UserContainerName"]);
            _statementContainer = database.GetContainer(cosmosConfig["StatementContainerName"]);
        }

        public async Task AddUserAsync(User user)
        {
            _logger.LogInformation("Adding user with UserId: {UserId}", user.UserId);
            try
            {
                await _userContainer.UpsertItemAsync(user, new PartitionKey(user.UserId));
                _logger.LogInformation("User added successfully: {UserId}", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user: {UserId}", user.UserId);
                throw;
            }
        }

        public async Task AddStatementAsync(Statement statement)
        {
            await _statementContainer.UpsertItemAsync(statement, new PartitionKey(statement.UserId));
            _logger.LogInformation("Statement added for user: {UserId}", statement.UserId);
        }

        public async Task<List<Statement>> GetStatementsByUserIdAsync(string userId)
        {
            _logger.LogInformation("Fetching statements for UserId: {UserId}", userId);
            var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
                .WithParameter("@userId", userId);
            _logger.LogDebug("Executing query: {QueryText}", query.QueryText);
            try
            {
                var iterator = _statementContainer.GetItemQueryIterator<Statement>(query);
                var results = await iterator.ReadNextAsync();
                var statements = results.ToList();
                _logger.LogInformation("Found {Count} statements for UserId: {UserId}", statements.Count, userId);
                return statements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch statements for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Username = @username")
                .WithParameter("@username", username);
            try
            {
                var iterator = _userContainer.GetItemQueryIterator<User>(query);
                var results = await iterator.ReadNextAsync();
                var user = results.FirstOrDefault();
                if (user == null)
                {
                    _logger.LogWarning("No user found for Username: {Username}", username);
                }
                else
                {
                    _logger.LogInformation("Found user: {UserId}", user.UserId);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch user for Username: {Username}", username);
                throw;
            }
        }

        public async Task SaveStatementAsync(Statement statement)
        {
            _logger.LogInformation("Saving statement with StatementId: {StatementId} for UserId: {UserId}", statement.StatementId, statement.UserId);
            try
            {
                await _statementContainer.UpsertItemAsync(statement, new PartitionKey(statement.UserId));
                _logger.LogInformation("Statement saved successfully: {StatementId}", statement.StatementId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save statement: {StatementId}", statement.StatementId);
                throw;
            }
        }

        public async Task<AuthResult> AuthenticateUserAsync(string username, string password)
        {
            // Fetch user from Cosmos DB—null if not found.
            var user = await GetUserByUsernameAsync(username);

            // No user—return result with UserFound false.
            if (user == null)
            {
                return new AuthResult { UserFound = false, PasswordMatch = false };
            }

            // User found—check password match.
            var passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);
            return new AuthResult
            {
                User = passwordMatch ? user : null,
                UserFound = true,
                PasswordMatch = passwordMatch
            };
        }
    }
}