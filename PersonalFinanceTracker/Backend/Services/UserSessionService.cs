using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Common.Models;

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
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            CurrentUser = new User { UserId = userId, Username = username };
        }

        public void ClearUser()
        {
            CurrentUser = null;
        }
    }
}
