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

        // Asynchronous method to handle the sign-up process
        public async Task<bool> SignUpAsync()
        {
            bool valid = true;

            // Validate the username field
            UsernameError = string.IsNullOrWhiteSpace(Username)
                ? "Please enter a username."  // Error if the username is empty or whitespace
                : await _userRepository.UserExistsAsync(Username)
                    ? "Username already exists."  // Error if the username already exists in the database
                    : string.Empty;  // No error if validation passes
            valid &= UsernameError == string.Empty;  // Update 'valid' based on username validation

            // Validate the email field
            EmailError = IsValidEmail(Email)
                ? string.Empty  // No error if the email is valid
                : "Please enter a valid email address.";  // Error if the email is invalid
            valid &= EmailError == string.Empty;  // Update 'valid' based on email validation

            // Validate the password field
            PasswordError = ValidatePassword(Password)
                ? string.Empty  // No error if the password meets security criteria
                : "Password must be at least 8 characters, include a number, uppercase, and special character.";
            valid &= PasswordError == string.Empty;  // Update 'valid' based on password validation

            // Validate the confirm password field
            ConfirmPasswordError = Password == ConfirmPassword
                ? string.Empty  // No error if the passwords match
                : "Passwords do not match.";  // Error if they do not match
            valid &= ConfirmPasswordError == string.Empty;  // Update 'valid' based on confirm password validation

            // If all validations pass, create a new user and add them to the database
            if (valid)
            {
                // Create a new User object with provided data
                var user = new User { Username = Username, Email = Email, Password = Password };

                // Asynchronously save the user to the repository
                await _userRepository.AddUserAsync(user);
            }

            // Return the result of the validation checks
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
