using PersonalFinanceTracker.Backend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Services
{
    public class UserSessionService : IUserSessionService
    {
        public string UserId { get; private set; }
        public string Username { get; private set; }
        public bool IsUserLoggedIn => !string.IsNullOrEmpty(UserId);

        public void SetUser(string userId, string username)
        {
            UserId = userId;
            Username = username;
        }

        public void ClearUser()
        {
            UserId = null;
            Username = null;
        }

    }
}
