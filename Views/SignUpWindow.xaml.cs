using PersonalFinanceTracker.Backend.Models;
using PersonalFinanceTracker.Backend.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PersonalFinanceTracker.Views
{
    public partial class SignUpWindow : Window, INotifyPropertyChanged
    {
        private string _passwordText;
        public string PasswordText
        {
            get => _passwordText;
            set
            {
                if (_passwordText != value)
                {
                    _passwordText = value;
                    OnPropertyChanged(nameof(PasswordText)); // Notify UI that the PasswordText property has changed
                }
            }
        }

        private string _confirmPasswordText;
        public string ConfirmPasswordText
        {
            get => _confirmPasswordText;
            set
            {
                if (_confirmPasswordText != value)
                {
                    _confirmPasswordText = value;
                    OnPropertyChanged(nameof(ConfirmPasswordText)); // Notify UI that the PasswordText property has changed
                }
            }
        }

        private readonly UserRepository _userRepository;

        public SignUpWindow(UserRepository userRepository)
        {
            InitializeComponent();
            _userRepository = userRepository;
        }

        private void HighlightPasswordBox(PasswordBox passwordBox, string hexColor)
        {
            // Convert the hex color to a SolidColorBrush
            var colorBrush = new BrushConverter().ConvertFromString(hexColor) as SolidColorBrush;

            // Change BorderBrush temporarily
            passwordBox.BorderBrush = colorBrush;

            // Define the event handler
            RoutedEventHandler resetBorderBrushHandler = null;
            resetBorderBrushHandler = (s, ev) =>
            {
                // Reset BorderBrush to default color
                passwordBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#899878"));

                // Unsubscribe the handler after resetting
                passwordBox.PasswordChanged -= resetBorderBrushHandler;
            };

            // Subscribe the handler
            passwordBox.PasswordChanged += resetBorderBrushHandler;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;

            // Create user object from the form or data entered
            var user = new User
            {
                Username = UsernameField.Text,
                Email = EmailField.Text,
                Password = PasswordField.Password
            };

            //password confirmation check
            if (PasswordText != ConfirmPasswordText)
            {
                // Clear password fields
                PasswordField.Password = string.Empty;
                ConfirmPasswordField.Password = string.Empty;

                // Apply highlight
                HighlightPasswordBox(PasswordField, "#5A1807");
                HighlightPasswordBox(ConfirmPasswordField, "#5A1807");

                ErrorField.Text = "Passwords do not match.";

                valid = false;
            }

            // Username validation
            if (await _userRepository.UserExistsAsync(user.Username))
            {
                ErrorField.Text = "Username already exists. Please choose another one.";
                //MessageBox.Show("Username already exists. Please choose another one.");
                valid = false;
            }

            // Email validation
            if (!IsValidEmail(EmailField.Text))
            {
                MessageBox.Show("Please enter a valid email.");
                valid = false;
            }

            // Password validation
            if (!ValidatePassword(PasswordText))
            {
                valid = false;
            }

            // Check if the password and re-entered password match
            if (PasswordText != ConfirmPasswordText)
            {
                MessageBox.Show("Passwords do not match.");
                valid = false;
            }

            if (valid) 
            {
                await _userRepository.AddUserAsync(user); // Adds user to DynamoDB
            }
        }

        // Removes placeholder text for the password field when the user begins typing
        private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender; // Casts the sender to a PasswordBox

            PasswordText = passwordBox.Password;

            // Update the placeholder text based on whether the password is empty
            if (!string.IsNullOrEmpty(PasswordText))
            {
                PasswordPlaceholderText.Text = "";
            }
            else
            {
                PasswordPlaceholderText.Text = "Enter your password";
            }
        }

        // Removes placeholder text for the confirmation password field when the user begins typing
        private void ConfirmPasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender; // Casts the sender to a PasswordBox

            ConfirmPasswordText = passwordBox.Password;

            // Update the placeholder text based on whether the password is empty
            if (!string.IsNullOrEmpty(ConfirmPasswordText))
            {
                ConfirmPasswordPlaceholderText.Text = "";
            }
            else
            {
                ConfirmPasswordPlaceholderText.Text = "Enter your password";
            }
        }

        // Ensures the password meets strength requirements
        public bool ValidatePassword(string password)
        {
            // Check if password is at least 8 characters long
            if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.");
                return false;
            }

            // Check if password contains at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                MessageBox.Show("Password must contain at least one uppercase letter.");
                return false;
            }

            // Check if password contains at least one number
            if (!Regex.IsMatch(password, @"\d"))
            {
                MessageBox.Show("Password must contain at least one number.");
                return false;
            }

            // Check if password contains at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                MessageBox.Show("Password must contain at least one special symbol.");
                return false;
            }

            return true;
        }

        // Checks the users email and ensures the format is valid
        public bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return emailRegex.IsMatch(email);
        }

       
    }
}
