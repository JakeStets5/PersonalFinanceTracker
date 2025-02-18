using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using PersonalFinanceTracker.Backend.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Reflection.Metadata;
using PersonalFinanceTracker.Views;
using System.Windows.Media.Imaging;
using BCrypt.Net;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Backend.Commands;
using System.Runtime.CompilerServices;

namespace PersonalFinanceTracker.ViewModels
{
    public class SignUpViewModel : INotifyPropertyChanged
    {
        private readonly IUserRepository _userRepository;
        private readonly IPFTDialogService _dialogService;

        private string _username = "";
        private string _usernameError = "";
        private string _email = "";
        private string _emailError = "";
        private string _password = "";
        private string _tempPassword = "";
        private string _passwordError = "";
        private string _confirmPassword = "";
        private string _tempConfirmPassword = "";
        private string _confirmPasswordError = "";
        private bool _isPasswordVisible;
        private bool _isConfirmPasswordVisible;
        private bool _isUpdating = false;
        private bool _valid = true; // For credential verification

        public bool IsPasswordPlaceholderVisible => string.IsNullOrEmpty(Password); // To make the placeholder text for password vanish
        public bool IsConfirmPasswordPlaceholderVisible => string.IsNullOrEmpty(ConfirmPassword); // To make the placeholder text for confirm password vanish
        public char PasswordMaskChar => IsPasswordVisible ? '\0' : '●';


        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? OnSignUpCompleted; // Event that notifies of completion

        public SignUpViewModel(IUserRepository userRepository, IPFTDialogService dialogService)
        {
            _dialogService = dialogService;
            _userRepository = userRepository;

            // When the sign up button is clicked...
            SignUpCommand = new RelayCommand(async () => await SignUpAsync());

            // When the password visibility button is clicked...
            TogglePasswordVisibilityCommand = new RelayCommand<Button>(TogglePasswordVisibility);

            // When the confirm password visibility button is clicked...
            ToggleConfirmPasswordVisibilityCommand = new RelayCommand<Button>(ToggleConfirmPasswordVisibility);

            // When either password is changed...
            PasswordChangedCommand = new RelayCommand<object>(parameter => HandlePasswordChanged(parameter));
            _dialogService = dialogService;
        }

        // Commands
        public ICommand SignUpCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand PasswordChangedCommand { get; } // Must include due to password box not supporting two way binding

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
            set
            {
                if (SetProperty(ref _password, value))
                {
                    OnPropertyChanged(nameof(IsPasswordPlaceholderVisible)); // Notify UI of placeholder visibility change
                }
            }
        }

        public string TempPassword
        {
            get => _tempPassword;
            set
            {
                if (SetProperty(ref _tempPassword, value))
                {
                    OnPropertyChanged(nameof(IsPasswordPlaceholderVisible)); // Notify UI of placeholder visibility change
                }
            }
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    OnPropertyChanged(nameof(IsConfirmPasswordPlaceholderVisible)); // Notify UI of placeholder visibility change
                }
            }
        }

        public string ConfirmTempPassword
        {
            get => _tempConfirmPassword;
            set
            {
                if (SetProperty(ref _tempConfirmPassword, value))
                {
                    OnPropertyChanged(nameof(IsConfirmPasswordPlaceholderVisible)); // Notify UI of placeholder visibility change
                }
            }
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

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set => SetProperty(ref _isConfirmPasswordVisible, value);
        }

        // Method to handle when the user updates the password fields
        private void HandlePasswordChanged(object parameter)
        {
            if (parameter is object[] values && values.Length == 3)
            {
                var passwordBox = values[0] as PasswordBox;
                var textBox = values[1] as TextBox;
                var identifier = values[2] as string;

                if (passwordBox == null || textBox == null || string.IsNullOrEmpty(identifier)) return;

                if (_isUpdating) return;
                _isUpdating = true;

                if (passwordBox.IsFocused)
                {
                    if (identifier == "MainPassword")
                        Password = passwordBox.Password;
                    else if (identifier == "ConfirmPassword")
                        ConfirmPassword = passwordBox.Password;

                    textBox.Text = passwordBox.Password; // Manually sync to TextBox
                }
                else if (textBox.IsFocused)
                {
                    if (identifier == "MainPassword")
                        Password = textBox.Text;
                    else if (identifier == "ConfirmPassword")
                        ConfirmPassword = textBox.Text;

                    passwordBox.Password = textBox.Text; // Manually sync to PasswordBox
                }

                _isUpdating = false;
            }
        }

        // Asynchronous method to handle the sign-up process
        public async Task<bool> SignUpAsync()
        {
            _valid = true;
            // Validate the username field
            if (string.IsNullOrWhiteSpace(Username))
                UsernameError = "Please enter a username.";
            else if (await _userRepository.UserExistsAsync(Username))
                UsernameError = "Username already exists."; // Error if the username already exists in the database
            else
                UsernameError = string.Empty;  // No error if validation passes

            _valid &= UsernameError == string.Empty;  // Update 'valid' based on username validation

            // Validate the email field
            if (string.IsNullOrWhiteSpace(Email)) 
                EmailError = "Please enter an email address";
            else if(!IsValidEmail(Email))
                EmailError = "Please enter a valid email address."; // Error if email format is wrong
            else
                EmailError = string.Empty;

            _valid &= EmailError == string.Empty;  // Update 'valid' based on email validation

            // Validate the password field
            if (string.IsNullOrWhiteSpace(Password))
                PasswordError = "Please enter a password";
            else if (!ValidatePassword(Password))
                PasswordError = "Password must be at least 8 characters, include a number, uppercase letter, and special character.";
            else
                PasswordError = string.Empty;  // No error if the password meets security criteria

            _valid &= PasswordError == string.Empty;  // Update 'valid' based on password validation

            // Validate the confirm password field
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
                ConfirmPasswordError = "Please confirm your password";
            else if (Password != ConfirmPassword)
                ConfirmPasswordError = "Passwords do not match.";  // Error if passwords do not match
            else
                ConfirmPasswordError = string.Empty;  // No error if the passwords match

            _valid &= ConfirmPasswordError == string.Empty;  // Update 'valid' based on confirm password validation

            // If all validations pass, create a new user and add them to the database
            if (_valid)
            {
                // Hash the password using BCrypt for security
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                // Create a new User object with provided data
                var user = new User { Username = Username, Email = Email, Password = hashedPassword };

                // Asynchronously save the user to the repository
                try
                {
                    await _userRepository.AddUserAsync(user);
                }
                catch (Exception ex)
                {
                    // Handle errors if needed
                    Console.WriteLine(ex.Message);
                }

                // Show success pop-up and close the sign-up window
                OnSignUpCompleted?.Invoke();
                _dialogService.ShowSuccessMessage("Sign Up Successful!");
            }

            // Return the result of the validation checks
            return _valid;
        }

        // Ensures the password reaches the requirements
        private bool ValidatePassword(string password) =>
            password.Length >= 8 &&
            Regex.IsMatch(password, @"[A-Z]") &&
            Regex.IsMatch(password, @"\d") &&
            Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

        // Ensures the email meets requirements
        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");

        private void TogglePasswordVisibility(Button button)
        {
            if (IsPasswordVisible)
            {
                // Switching from visible (TextBox) to hidden (PasswordBox)
                Password = TempPassword;  // Save any text input in visible mode
                OnPropertyChanged(nameof(Password));
            }
            else
            {
                // Switching from hidden (PasswordBox) to visible (TextBox)
                TempPassword = Password;  // Store current password value to display
                
                OnPropertyChanged(nameof(TempPassword));
            }

            // Toggle the boolean property
            IsPasswordVisible = !IsPasswordVisible;

            // Find the Image inside the Button
            if (button.Content is Image image)
            {
                string newIconPath = IsPasswordVisible
                    ? "../Assets/Images/SignUpWindowIcons/open_eye_icon.png"
                    : "../Assets/Images/SignUpWindowIcons/closed_eye_icon.png";

                image.Source = new BitmapImage(new Uri(newIconPath, UriKind.Relative));
            }
        }

        private void ToggleConfirmPasswordVisibility(Button button)
        {
            if (IsConfirmPasswordVisible)
            {
                // Switching from visible (TextBox) to hidden (PasswordBox)
                ConfirmPassword = ConfirmTempPassword;  // Save any text input in visible mode
                OnPropertyChanged(nameof(ConfirmPassword));
            }
            else
            {
                // Switching from hidden (PasswordBox) to visible (TextBox)
                ConfirmTempPassword = ConfirmPassword;  // Store current password value to display

                OnPropertyChanged(nameof(ConfirmTempPassword));
            }

            // Toggle the boolean property
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;

            // Find the Image inside the Button
            if (button.Content is Image image)
            {
                string newIconPath = IsConfirmPasswordVisible
                    ? "../Assets/Images/SignUpWindowIcons/open_eye_icon.png"
                    : "../Assets/Images/SignUpWindowIcons/closed_eye_icon.png";

                image.Source = new BitmapImage(new Uri(newIconPath, UriKind.Relative));
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            if(propertyName != null)
            {
                OnPropertyChanged(propertyName);
            }

            // Notify Command State Updates
            (SignUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
            return true;
        }
    }
}
