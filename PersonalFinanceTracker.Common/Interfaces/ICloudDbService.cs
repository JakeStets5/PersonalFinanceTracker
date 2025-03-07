using PersonalFinanceTracker.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Common.Interfaces
{
    public interface ICloudDbService
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserByUsernameAsync(string userId);
        Task SaveStatementAsync(Statement statement);
        public Task<IEnumerable<Statement>> GetStatementsByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<AuthResult> AuthenticateUserAsync(string username, string password);
        public Task SaveRawStatementAsync(string json); // for testing
    }
}
