using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Backend.Models;
using PersonalFinanceTracker.Backend.Repositories;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Reflection.Metadata;

namespace PersonalFinanceTracker.ViewModels
{
    public class SignUpViewModel : INotifyPropertyChanged
    {
        private readonly UserRepository _userRepository;

        private string _username;
        private string _usernameError;
        private string _email;
        private string _emailError;
        private string _password;
        private string _passwordError;
        private string _confirmPassword;
        private string _confirmPasswordError;
        private bool _isPasswordVisible;
        public bool IsPasswordPlaceholderVisible => string.IsNullOrEmpty(Password); // To make the placeholder text for password vanish
        public bool IsConfirmPasswordPlaceholderVisible => string.IsNullOrEmpty(ConfirmPassword); // To make the placeholder text for confirm password vanish

        public event PropertyChangedEventHandler? PropertyChanged;

        public SignUpViewModel(UserRepository userRepository)
        {
            _userRepository = userRepository;

            // Corrected Commands
            SignUpCommand = new RelayCommand(async () => await SignUpAsync());
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);

            // When the password is changed...
            PasswordChangedCommand = new RelayCommand<object>(parameter => 
            {
                if (parameter is PasswordBox passwordBox)
                {
                    Password = passwordBox.Password;
                    OnPropertyChanged(nameof(IsPasswordPlaceholderVisible));
                }
            });

            // When the confirm password is changed...
            ConfirmPasswordChangedCommand = new RelayCommand<object>(parameter =>
            {
                if (parameter is PasswordBox passwordBox)
                {
                    ConfirmPassword = passwordBox.Password;
                    OnPropertyChanged(nameof(IsConfirmPasswordPlaceholderVisible));
                }
            });
        }
         
        // Properties and their Notifications
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string UsernameError
        {
            get => _usernameError;
            set => SetProperty(ref _usernameError, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        
        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ConfirmPasswordError 
        {
            get => _confirmPasswordError;
            set => SetProperty(ref _confirmPasswordError, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        // Commands
        public ICommand SignUpCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand PasswordChangedCommand { get; } // Must include due to password box not supporting two way binding
        public ICommand ConfirmPasswordChangedCommand { get; } // Must include due to password box not supporting two way binding

        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        // Asynchronous method to handle the sign-up process
        public async Task<bool> SignUpAsync()
        {
            bool valid = true;

            // Validate the username field
            if (string.IsNullOrWhiteSpace(Username))
                UsernameError = "Please enter a username.";
            else if (await _userRepository.UserExistsAsync(Username))
                UsernameError = "Username already exists."; // Error if the username already exists in the database
            else
                UsernameError = string.Empty;  // No error if validation passes

            valid &= UsernameError == string.Empty;  // Update 'valid' based on username validation

            // Validate the email field
            if (string.IsNullOrWhiteSpace(Email)) 
                EmailError = "Please enter an email address";
            else if(!IsValidEmail(Email))
                EmailError = "Please enter a valid email address."; // Error if email format is wrong
            else
                EmailError = string.Empty;

            valid &= EmailError == string.Empty;  // Update 'valid' based on email validation

            // Validate the password field
            if (string.IsNullOrWhiteSpace(Password))
                PasswordError = "Please enter a password";
            else if (!ValidatePassword(Password))
                PasswordError = "Password must be at least 8 characters, include a number, uppercase letter, and special character.";
            else
                PasswordError = string.Empty;  // No error if the password meets security criteria

            valid &= PasswordError == string.Empty;  // Update 'valid' based on password validation

            // Validate the confirm password field
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
                ConfirmPasswordError = "Please confirm your password";
            else if (Password != ConfirmPassword)
                ConfirmPasswordError = "Passwords do not match.";  // Error if passwords do not match
            else
                ConfirmPasswordError = string.Empty;  // No error if the passwords match

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

            // Notify Command State Updates
            (SignUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
            return true;
        }
    }
}
