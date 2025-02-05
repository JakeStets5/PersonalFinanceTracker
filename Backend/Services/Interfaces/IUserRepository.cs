using PersonalFinanceTracker.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Services.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<bool> UserExistsAsync(string username);
        Task<User?> GetUserByIdAsync(string userId);
    }
}
