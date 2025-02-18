using PersonalFinanceTracker.Backend.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using PersonalFinanceTracker.Backend.Interfaces;

namespace PersonalFinanceTracker.Backend.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly IAwsDynamoDbService _dynamoDbService;
        private readonly Lazy<ILogger<UserRepository>> _logger;
        private readonly DynamoDBContext _context;

        public UserRepository(IAwsDynamoDbService dynamoDbService, Lazy<ILogger<UserRepository>> logger, DynamoDBContext context)
        {
            _dynamoDbService = dynamoDbService;
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                _logger.Value.LogInformation("Attempting to add user with ID: {UserId} and Email: {Email}", user.Username, user.Email);

                // Call the service to add the user to DynamoDB
                await _dynamoDbService.AddUserAsync(user);

                _logger.Value.LogInformation("User {UserId} added successfully to DynamoDB", user.Username);
            }
            catch (Exception ex)
            {
                _logger.Value.LogError(ex, "Error occurred while adding user {UserId}", user.Username);
            }
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var existingUser = await _context.LoadAsync<User>(username);
            return existingUser != null;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.LoadAsync<User>(username);
        }
    }
}
