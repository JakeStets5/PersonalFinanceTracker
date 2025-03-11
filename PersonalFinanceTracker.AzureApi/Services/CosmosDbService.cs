using Microsoft.Azure.Cosmos;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;
using User = PersonalFinanceTracker.Common.Models.User;

namespace PersonalFinanceTracker.AzureApi.Services
{
    public class CosmosDbService : ICloudDbService
    {
        private readonly CosmosClient _cosmosClient; // object with methods that makes the API calls
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

        /// <summary>
        /// Upserts a user to Cosmos DB asynchronously.
        /// </summary>
        /// <param name="user">The user to submit</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        public async Task AddUserAsync(User user)
        {
            try
            {
                // No user Id in the user to be saved
                if (string.IsNullOrEmpty(user.UserId))
                {
                    throw new ArgumentException("UserId and StatementId must not be null or empty");
                }
                // Prepare the data for transport
                var json = System.Text.Json.JsonSerializer.Serialize(user);

                // Upsert (upload/insert) user to specified cosmos container
                await _userContainer.UpsertItemAsync(user, new PartitionKey(user.UserId));
                _logger.LogInformation("User upserted with UserId: {UserId}", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user: {UserId}", user.UserId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all statements from Cosmos DB by their userId asynchronously.
        /// </summary>
        /// <param name="userId">The userId to search by</param>
        /// /// <param name="startDate">date filter</param>
        /// /// <param name="endDate">date filter</param>
        /// <returns>A <see cref="Statement"/> enumerable list if found. Else, null.</returns>
        /// <exception cref="Exception">Thrown if the query fails, with details logged.</exception>
        /// <remarks>
        /// Queries the 'Statements' container using a parameterized SQL query. Logs success or failure.
        /// </remarks>
        public async Task<IEnumerable<Statement>> GetStatementsByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var queryText = "SELECT * FROM c WHERE c.UserId = @userId";
            var parameters = new Dictionary<string, object> { { "@userId", userId } };

            // Add date filters. Cosmos uses ISO 8601 strings
            if (startDate.HasValue)
            {
                queryText += " AND c.Date >= @startDate";
                // Uses offset format to match Cosmos
                parameters.Add("@startDate", startDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
            }
            if (endDate.HasValue)
            {
                queryText += " AND c.Date <= @endDate";
                // End of day: 23:59:59.9999999 (default is like 6am)
                var endOfDay = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(9999999);
                parameters.Add("@endDate", endOfDay.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
            }

            // Create and add parameters to the query
            var query = new QueryDefinition(queryText);
            foreach (var param in parameters)
            {
                query = query.WithParameter(param.Key, param.Value);
            }

            var iterator = _statementContainer.GetItemQueryIterator<Statement>(query);
            var statements = new List<Statement>();

            try
            {
                // Fetch all statements from Cosmos DB
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    statements.AddRange(response);
                }
                return statements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch statements for UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a user from Cosmos DB by their username asynchronously.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>A <see cref="User"/> object if found; otherwise, null.</returns>
        /// <exception cref="Exception">Thrown if the query fails, with details logged.</exception>
        /// <returns>A <see cref="User"/> The user object if found, or null if not found or an error occurred.</returns>
        /// <remarks>
        /// Queries the 'Users' container using a parameterized SQL query. Logs success or failure.
        /// </remarks>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Username = @username")
                .WithParameter("@username", username);
            try
            {
                var iterator = _userContainer.GetItemQueryIterator<User>(query);
                var results = await iterator.ReadNextAsync();
                var user = results.FirstOrDefault(); // Get the first user found

                // Log if user is not found
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

        /// <summary>
        /// Upserts a statement to Cosmos DB asynchronously.
        /// </summary>
        /// <param name="statement">The statement to submit.</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        /// <remarks>
        /// Usperts the statement to the 'Statements' container in Cosmos DB. Logs success or failure.
        /// </remarks>
        public async Task SaveStatementAsync(Statement statement)
        {
            try
            {
                // No user or statement Id in the statenent to be saved
                if (string.IsNullOrEmpty(statement.UserId) || string.IsNullOrEmpty(statement.StatementId))
                {
                    throw new ArgumentException("UserId and StatementId must not be null or empty");
                }

                // Add userId and statementId to set partition key (heirarchieal keys in cosmos)
                var partitionKey = new PartitionKeyBuilder()
                    .Add(statement.UserId)
                    .Add(statement.StatementId)
                    .Build();

                // Upsert (upload/insert) statement to specified cosmos container. May be redundant considering the next line?
                await _statementContainer.UpsertItemAsync(statement, partitionKey);
                _logger.LogInformation("Statement upserted: {StatementId} for UserId: {UserId}",
                    statement.StatementId, statement.UserId);

                // Fetch response from cosmos after upserting (upload/insert) statement to specified cosmos container
                var cosmosResponse = await _statementContainer.UpsertItemAsync(statement, partitionKey);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error upserting statement: {StatementId} for UserId: {UserId}",
                    statement.StatementId ?? "null", statement.UserId ?? "null");
                throw;
            }
        }

        /// <summary>
        /// Ensures the user is authenticated and the password matches the stored hash.
        /// </summary>
        /// <param name="username">The username submitted by the user.</param>
        /// <param name="password">The password submitted by the user.</param>
        /// <exception cref="Exception">Thrown if the upsert fails, with details logged.</exception>
        /// <returns>A <see cref="AuthResult"/></returns>
        /// <remarks>
        /// Holds user info regarding whether the user was found and the password matched.
        /// </remarks>
        public async Task<AuthResult> AuthenticateUserAsync(string username, string password)
        {
            // Fetch user from Cosmos DB. Null if not found
            var user = await GetUserByUsernameAsync(username);

            // No user. Return result with UserFound false
            if (user == null)
            {
                return new AuthResult { UserFound = false, PasswordMatch = false };
            }

            // User found. Verify password with BCrypt
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