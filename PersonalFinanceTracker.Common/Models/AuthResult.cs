using PersonalFinanceTracker.Common.Models;

namespace PersonalFinanceTracker.Common.Models
{
    public class AuthResult
    {
        public User? User { get; set; }
        public bool UserFound { get; set; }
        public bool PasswordMatch { get; set; }
    }
}