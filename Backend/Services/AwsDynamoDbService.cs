using PersonalFinanceTracker.Backend.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using PersonalFinanceTracker.Backend.Interfaces;

namespace PersonalFinanceTracker.Backend.Services
{
    public class AwsDynamoDbService : IAwsDynamoDbService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly ILogger<AwsDynamoDbService> _logger;
        private readonly string _tableName = "Users"; // Replace with your table name

        public AwsDynamoDbService(IAmazonDynamoDB dynamoDb, ILogger<AwsDynamoDbService> logger)
        {
            _dynamoDb = dynamoDb;
            _logger = logger;
        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                _logger.LogInformation("Attempting to add user to DynamoDB with ID: {Username}", user.Username);

                var table = Table.LoadTable(_dynamoDb, _tableName);
                var document = new Document
                {
                    ["Username"] = user.Username,
                    ["Email"] = user.Email,
                    ["Password"] = user.Password,
                };

                await table.PutItemAsync(document);

                _logger.LogInformation("User with ID {Username} added successfully to DynamoDB", user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user to DynamoDB with ID {Username}", user.Username);
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            // Example of how to get a user by ID
            var table = Table.LoadTable(_dynamoDb, _tableName);
            var document = await table.GetItemAsync(userId);
            if (document != null)
            {
                return new User
                {
                    Username = document["Username"],
                    Email = document["Email"]
                    // Map other properties here
                };
            }
            return null;
        }
    }
}
