using System;
using System.Collections.Generic;
using System.ComponentModel;
using PersonalFinanceTracker.Views;
using PersonalFinanceTracker.Common.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Backend.Commands;
using System.Diagnostics;

namespace PersonalFinanceTracker.ViewModels
{
    public class SignInViewModel : BindableBase, INotifyPropertyChanged
    {
        private string _username = ""; 
        private string _usernameError = "";
        private string _password = "";
        private string _tempPassword = "";
        private string _passwordError = "";
        private bool _isPasswordVisible = false;
        private bool _valid = true;
        private bool _isUpdating = false;
        private bool _isSigningIn; // Prevent double calls

        private readonly INavigationService _navigationService; // To help with window navigation

        private readonly IPFTDialogService _dialogService; // For any pop up dialog

        private readonly IUserSessionService _userSessionService; // To help with user session management

        private readonly IApiClient _apiClient; // To help with API calls

        public bool IsPasswordPlaceholderVisible => string.IsNullOrEmpty(TempPassword); // To make the placeholder text for password vanish
        public char PasswordMaskChar => IsPasswordVisible ? '\0' : '●';

        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? CloseAction { get; set; } // Delegate to close the window

        public ICommand SignInCommand { get; }
        public ICommand OpenSignUpCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

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

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged(nameof(IsPasswordVisible));
                OnPropertyChanged(nameof(PasswordMaskChar));
            }
        }

        public SignInViewModel(INavigationService navigationService, IPFTDialogService dialogService, IUserSessionService userSessionService, IApiClient apiClient)
        {
            Debug.WriteLine("SignInViewModel constructor called");
            _apiClient = apiClient;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            // When the password is changed...
            PasswordChangedCommand = new RelayCommand<object>(HandlePasswordChanged);

            // When the Sign up link is clicked...
            OpenSignUpCommand = new RelayCommand(OpenSignUp);

            // When the Sign In button is clicked...
            SignInCommand = new RelayCommand(async () => await SignInAsync());

            // When the eye icon is clicked...
            TogglePasswordVisibilityCommand = new RelayCommand<Button>(TogglePasswordVisibility);
        }

        private void HandlePasswordChanged(object parameter)
        {
            // Extract the PasswordBox and TextBox from the parameter
            if (parameter is object[] values && values.Length == 2)
            {
                var passwordBox = values[0] as PasswordBox;
                var textBox = values[1] as TextBox;

                if (textBox != null && passwordBox != null)
                {
                    if (_isUpdating) return; // Prevent infinite loop
                    _isUpdating = true;

                    if (passwordBox.IsFocused)
                    {
                        TempPassword = passwordBox.Password;
                        textBox.Text = TempPassword; // Manually sync to TextBox
                    }
                    else if (textBox.IsFocused)
                    {
                        TempPassword = textBox.Text;
                        passwordBox.Password = TempPassword; // Manually sync to PasswordBox
                    }

                    _isUpdating = false;
                }
            }
        }

        private void TogglePasswordVisibility(Button button)
        {
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

        // Handles the sign up link click. Opens the sign up window
        private void OpenSignUp()
        {
            _navigationService.OpenSignUpWindow();
            CloseAction?.Invoke();
        }

        // Handles the sign in button click
        private async Task<bool> SignInAsync()
        {
            if (_isSigningIn) return false; // Block double calls
            _isSigningIn = true;
            ((RelayCommand)SignInCommand).RaiseCanExecuteChanged();

            try
            {
                Console.WriteLine($"SignInAsync started with Username: {Username}");
                _valid = true;

                // Validate the username field
                if (string.IsNullOrWhiteSpace(Username))
                    UsernameError = "Please enter a username.";
                else
                    UsernameError = string.Empty;

                _valid &= UsernameError == string.Empty;

                // Validate the password field
                if (string.IsNullOrWhiteSpace(Password))
                    PasswordError = "Please enter a password.";
                else
                    PasswordError = string.Empty;

                _valid &= PasswordError == string.Empty;

                if (!_valid) return false; // Stop if basic validation fails

                // Call the API to sign in
                var (user, error) = await _apiClient.SignInAsync(Username, Password);
                if (user == null)
                {
                    UsernameError = "Username not found.";
                    return false;
                }

                // Authentication successful
                Console.WriteLine($"Calling SetUser with {user.UserId}");
                _userSessionService.SetUser(user.UserId, user.Username);
                _dialogService.ShowSuccessMessage("Sign In Successful!");
                CloseAction?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sign-in: {ex.Message}");
                return false;
            }
            finally
            {
                // Reset the signing in flag
                ((RelayCommand)SignInCommand).RaiseCanExecuteChanged();
                _isSigningIn = false;
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
            }

            // Notify Command State Updates
            (SignInCommand as RelayCommand)?.RaiseCanExecuteChanged();
            return true;
        }
    }
}
