using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using PersonalFinanceTracker.Backend.Models;
using PersonalFinanceTracker.Backend.Repositories;

namespace PersonalFinanceTracker.ViewModels
{
    public class SignUpViewModel : INotifyPropertyChanged
    {
        private readonly UserRepository _userRepository;

        private string _username;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private bool _isPasswordVisible;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SignUpViewModel(UserRepository userRepository)
        {
            _userRepository = userRepository;

            // Corrected Commands
            SignUpCommand = new RelayCommand(async () => await SignUpAsync(), CanSignUp);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);

            // Accepts Password as string
            PasswordChangedCommand = new RelayCommand<string>(p => Password = p);
            ConfirmPasswordChangedCommand = new RelayCommand<string>(cp => ConfirmPassword = cp);
        }

        // Properties with Notifications
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        // Error Messages
        public string PasswordError { get; set; }
        public string ConfirmPasswordError { get; set; }
        public string UsernameError { get; set; }
        public string EmailError { get; set; }

        // Commands
        public ICommand SignUpCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        public ICommand ConfirmPasswordChangedCommand { get; }

        // Validation & Sign-Up Logic
        private bool CanSignUp() =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password) &&
            Password == ConfirmPassword;

        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        public async Task<bool> SignUpAsync()
        {
            bool valid = true;

            UsernameError = string.IsNullOrWhiteSpace(Username)
                ? "Please enter a username."
                : await _userRepository.UserExistsAsync(Username) ? "Username already exists." : null;
            valid &= UsernameError == null;

            EmailError = IsValidEmail(Email) ? null : "Please enter a valid email address.";
            valid &= EmailError == null;

            PasswordError = ValidatePassword(Password) ? null :
                "Password must be at least 8 characters, include a number, uppercase, and special character.";
            valid &= PasswordError == null;

            ConfirmPasswordError = Password == ConfirmPassword ? null : "Passwords do not match.";
            valid &= ConfirmPasswordError == null;

            if (valid)
            {
                var user = new User { Username = Username, Email = Email, Password = Password };
                await _userRepository.AddUserAsync(user);
            }

            return valid;
        }

        private bool ValidatePassword(string password) =>
            password.Length >= 8 &&
            Regex.IsMatch(password, @"[A-Z]") &&
            Regex.IsMatch(password, @"\d") &&
            Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
