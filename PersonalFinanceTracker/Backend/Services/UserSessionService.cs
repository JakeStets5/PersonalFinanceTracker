using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Services
{
    public class UserSessionService : IUserSessionService
    {
        

        public event Action<User?>? UserSignedIn;
        public string UserId { get; private set; }
        public string Username { get; private set; }
        public bool IsUserLoggedIn => !string.IsNullOrEmpty(UserId);

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                UserSignedIn?.Invoke(_currentUser);
            }
        }

        public void SetUser(string userId, string username)
        {
            UserId = userId;
            Username = username;
            CurrentUser = new User { UserId = userId, Username = username };
        }

        public void ClearUser()
        {
            CurrentUser = null;
        }

    }
}
