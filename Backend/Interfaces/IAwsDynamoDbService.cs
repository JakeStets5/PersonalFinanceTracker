using PersonalFinanceTracker.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Interfaces
{
    public interface IAwsDynamoDbService
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(string userId);
        Task SaveStatementAsync(Statement statement);
    }
}
