using System;
using System.Collections.Generic;
using System.ComponentModel;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Backend.Services.Interfaces;
using PersonalFinanceTracker.Views;
using PersonalFinanceTracker.Backend.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PersonalFinanceTracker.ViewModels
{
    public class SignInViewModel : INotifyPropertyChanged
    {
        private string _username = ""; 
        private string _usernameError = "";
        private string _password = "";
        private string _tempPassword = "";
        private string _passwordError = "";
        private bool _isPasswordVisible = false;
        private bool _valid = true;
        private bool _isUpdating = false;

        private readonly IUserRepository _userRepository;

        public bool IsPasswordPlaceholderVisible => string.IsNullOrEmpty(TempPassword); // To make the placeholder text for password vanish
        public char PasswordMaskChar => IsPasswordVisible ? '\0' : '●';

        public event PropertyChangedEventHandler? PropertyChanged;

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
                if (SetProperty(ref _password, value, nameof(Password)))
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
                if (SetProperty(ref _tempPassword, value, nameof(TempPassword)))
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

        public SignInViewModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            // When the password is changed...
            PasswordChangedCommand = new RelayCommand<object>(parameter =>
            {
                if (parameter is object[] values && values.Length == 2)
                {
                    var passwordBox = values[0] as PasswordBox;
                    var textBox = values[1] as TextBox;

                    if (textBox != null && passwordBox != null)
                    {
                        if (_isUpdating) return;
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
            });

            TogglePasswordVisibilityCommand = new RelayCommand<Button>(TogglePasswordVisibility);
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

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, string? propertyName = null)
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
