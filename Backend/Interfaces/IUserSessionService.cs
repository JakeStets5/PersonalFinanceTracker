using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Interfaces
{
    public interface IUserSessionService
    {
        string UserId { get; }
        string Username { get; }
        bool IsUserLoggedIn { get; }
        void SetUser(string userId, string username);
        void ClearUser();
    }
}
