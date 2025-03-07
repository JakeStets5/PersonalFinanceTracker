using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceTracker.Common.Models;

namespace PersonalFinanceTracker.Backend.Interfaces
{
    public interface IApiClient
    {
        public Task<User?> GetUserAsync(string username);
        public Task<(User? User, string? Error)> SignInAsync(string username, string password);
        public Task<Statement?> SubmitStatementAsync(Statement statement);
        public Task<IEnumerable<Statement?>> GetStatementsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
