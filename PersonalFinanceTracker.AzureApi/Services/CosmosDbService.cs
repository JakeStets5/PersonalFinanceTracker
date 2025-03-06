using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;
using Microsoft.Extensions.Logging; // For ILogger
using User = PersonalFinanceTracker.Common.Models.User;

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
            _logger.LogInformation("CosmosDbService constructor called");
            var cosmosConfig = config.GetSection("CosmosDb") ?? throw new ArgumentNullException(nameof(config));
            var dbName = cosmosConfig["DatabaseName"];
            if (string.IsNullOrEmpty(dbName)) throw new ArgumentException("DatabaseName is missing");
            if (string.IsNullOrEmpty(cosmosConfig["UserContainerName"])) throw new ArgumentException("UserContainerName is missing");
            if (string.IsNullOrEmpty(cosmosConfig["StatementContainerName"])) throw new ArgumentException("StatementContainerName is missing");
            var database = _cosmosClient.GetDatabase(dbName);
            _userContainer = database.GetContainer(cosmosConfig["UserContainerName"]);
            _statementContainer = database.GetContainer(cosmosConfig["StatementContainerName"]);
            _logger.LogInformation("CosmosDbService initialized with database: {DatabaseName}", dbName);
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
            _logger.LogInformation("Fetching user with Username: {Username}", username); // Note: Using userId as username here
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Username = @username")
                .WithParameter("@username", username);
            _logger.LogDebug("Executing query: {QueryText}", query.QueryText);
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
    }
}