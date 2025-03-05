
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Common.Models;
using PersonalFinanceTracker.Common.Interfaces;

namespace PersonalFinanceTracker.Backend.Services
{
    public class AwsDynamoDbService : ICloudDbService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly Lazy<ILogger<AwsDynamoDbService>> _logger;
        private readonly string _usersTable = "Users";
        private readonly string _statementsTable = "Statements"; 

        public AwsDynamoDbService(IAmazonDynamoDB dynamoDb, Lazy<ILogger<AwsDynamoDbService>> logger)
        {
            _dynamoDb = dynamoDb;
            _logger = logger;
        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                _logger.Value.LogInformation("Attempting to add user to DynamoDB with ID: {UserId}", user.Username);

                var table = Table.LoadTable(_dynamoDb, _usersTable);
                var document = new Document
                {
                    ["UserId"] = user.UserId,
                    ["Username"] = user.Username,
                    ["Email"] = user.Email,
                    ["Password"] = user.Password,
                };

                await table.PutItemAsync(document);

                _logger.Value.LogInformation("User with ID {Username} added successfully to DynamoDB", user.Username);
            }
            catch (Exception ex)
            {
                _logger.Value.LogError(ex, "Error occurred while adding user to DynamoDB with ID {Username}", user.Username);
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            // Example of how to get a user by ID
            var table = Table.LoadTable(_dynamoDb, _usersTable);
            var document = await table.GetItemAsync(userId);
            if (document != null)
            {
                return new User
                {
                    UserId = document["UserId"],
                    Username = document["Username"],
                    Email = document["Email"]
                    // Map other properties here
                };
            }
            return null;
        }

        public async Task SaveStatementAsync(Statement statement)
        {
            try
            {
                var table = Table.LoadTable(_dynamoDb, _statementsTable);
                var document = new Document
                {
                    ["UserId"] = statement.UserId,
                    ["StatementId"] = statement.StatementId,
                    ["Type"] = statement.Type,
                    ["Amount"] = statement.Amount,
                    ["Source"] = statement.Source,
                    ["Frequency"] = statement.Frequency,
                    ["Date"] = statement.Date,
                    ["PaymentMethod"] = statement.PaymentMethod
                };

                await table.PutItemAsync(document);

                _logger.Value.LogInformation("Statement with ID {StatementId} added successfully to DynamoDB", statement.StatementId);
            }
            catch
            {
                _logger.Value.LogError("Error occurred while adding statement to DynamoDB with ID {StatementId}", statement.StatementId);

            }
        }

        public async Task<List<Statement>> GetStatementsByUserIdAsync(string userId)
        {
            try
            {
                var table = Table.LoadTable(_dynamoDb, _statementsTable);

                // Query all statements where UserId matches
                var filter = new QueryFilter("UserId", QueryOperator.Equal, userId);
                var search = table.Query(filter);

                var statements = new List<Statement>();
                List<Document> documentList = await search.GetRemainingAsync();

                foreach (var document in documentList)
                {
                    statements.Add(new Statement
                    {
                        UserId = document["UserId"],
                        StatementId = document["StatementId"],
                        Type = document["Type"],
                        Amount = document["Amount"].AsDecimal(),
                        Source = document["Source"],
                        Frequency = document["Frequency"],
                        Date = document["Date"].AsDateTime(),
                        PaymentMethod = document["PaymentMethod"]
                    });
                }

                return statements;
            }
            catch (Exception ex)
            {
                _logger.Value.LogError(ex, "Error fetching statements for user {UserId}", userId);
                return new List<Statement>(); // Return empty list on failure
            }
        }
    }
}
