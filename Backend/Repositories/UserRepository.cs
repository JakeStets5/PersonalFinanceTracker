using PersonalFinanceTracker.Backend.Models;
using PersonalFinanceTracker.Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace PersonalFinanceTracker.Backend.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly IAwsDynamoDbService _dynamoDbService;
        private readonly ILogger<UserRepository> _logger;
        private readonly DynamoDBContext _context;

        public UserRepository(IAwsDynamoDbService dynamoDbService, ILogger<UserRepository> logger, DynamoDBContext context)
        {
            _dynamoDbService = dynamoDbService;
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                _logger.LogInformation("Attempting to add user with ID: {UserId} and Email: {Email}", user.Username, user.Email);

                // Call the service to add the user to DynamoDB
                await _dynamoDbService.AddUserAsync(user);

                _logger.LogInformation("User {UserId} added successfully to DynamoDB", user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding user {UserId}", user.Username);
            }
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var existingUser = await _context.LoadAsync<User>(username);
            return existingUser != null;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _dynamoDbService.GetUserByIdAsync(userId);
        }
    }
}
