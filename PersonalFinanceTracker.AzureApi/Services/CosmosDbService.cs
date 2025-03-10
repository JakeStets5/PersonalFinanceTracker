﻿using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;
using Microsoft.Extensions.Logging; // For ILogger
using User = PersonalFinanceTracker.Common.Models.User;
using BCrypt.Net;
using System.Text.Json;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

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

        public async Task<IEnumerable<Statement>> GetStatementsByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            _logger.LogInformation("Fetching statements for UserId: {UserId}", userId);
            var queryText = "SELECT * FROM c WHERE c.UserId = @userId";
            var parameters = new Dictionary<string, object> { { "@userId", userId } };

            // Add date filters if provided—Cosmos uses ISO 8601 strings.
            if (startDate.HasValue)
            {
                queryText += " AND c.Date >= @startDate";
                // Use offset format to match Cosmos
                parameters.Add("@startDate", startDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
                _logger.LogInformation("StartDate filter: {StartDate}", parameters["@startDate"]);
            }
            if (endDate.HasValue)
            {
                queryText += " AND c.Date <= @endDate";
                // End of day: 23:59:59.9999999
                var endOfDay = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                parameters.Add("@endDate", endOfDay.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
                _logger.LogInformation("EndDate filter: {EndDate}", parameters["@endDate"]);
            }

            var query = new QueryDefinition(queryText);
            foreach (var param in parameters)
            {
                query = query.WithParameter(param.Key, param.Value);
            }
            var iterator = _statementContainer.GetItemQueryIterator<Statement>(query);
            var statements = new List<Statement>();

            try
            {
                
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    _logger.LogInformation("Retrieved batch: {Batch}", System.Text.Json.JsonSerializer.Serialize(response));
                    statements.AddRange(response);
                }
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
            _logger.LogInformation("Target container: {ContainerId}", _statementContainer.Id);
            try
            {
                if (string.IsNullOrEmpty(statement.UserId) || string.IsNullOrEmpty(statement.StatementId))
                {
                    _logger.LogWarning("Invalid input: UserId: {UserId}, StatementId: {StatementId}",
                        statement.UserId, statement.StatementId);
                    throw new ArgumentException("UserId and StatementId must not be null or empty");
                }

                var partitionKey = new PartitionKeyBuilder()
                    .Add(statement.UserId)
                    .Add(statement.StatementId)
                    .Build();
                _logger.LogInformation("Raw PartitionKey: {PK}", partitionKey.ToString());

                var json = System.Text.Json.JsonSerializer.Serialize(statement);
                _logger.LogInformation("Saving statement: {Json}, PartitionKey: [{UserId}, {StatementId}]",
                    json, statement.UserId, statement.StatementId);

                await _statementContainer.UpsertItemAsync(statement, partitionKey);
                _logger.LogInformation("Statement upserted: {StatementId} for UserId: {UserId}",
                    statement.StatementId, statement.UserId);

                var cosmosResponse = await _statementContainer.UpsertItemAsync(statement, partitionKey);
                _logger.LogInformation("Statement upserted: {StatementId} for UserId: {UserId}, RU: {RU}, Resource: {Resource}",
                    statement.StatementId, statement.UserId, cosmosResponse.RequestCharge, JsonConvert.SerializeObject(cosmosResponse.Resource));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error upserting statement: {StatementId} for UserId: {UserId}",
                    statement.StatementId ?? "null", statement.UserId ?? "null");
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

        public Task SaveRawStatementAsync(string json)
        {
            throw new NotImplementedException();
        }
    }
}