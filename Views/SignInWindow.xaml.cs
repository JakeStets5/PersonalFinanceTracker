using PersonalFinanceTracker.Backend.Repositories;
using PersonalFinanceTracker.ViewModels;
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
    /// <summary>
    /// Interaction logic for SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window, INotifyPropertyChanged
    {
        private string _passwordText;
        public string PasswordText
        {
            get => _passwordText;
            set
            {
                _passwordText = value;
                OnPropertyChanged(nameof(PasswordText));
            }
        }

        private readonly UserRepository _userRepository;

        public SignInWindow(UserRepository userRepository)
        {
            InitializeComponent();
            _userRepository = userRepository;

            // Set window position relative to the main window
            CenterToMainWindow();
        }

        // Centers the sign in window in the main window
        private void CenterToMainWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Window mainWindow = Application.Current.MainWindow;

                // Calculate center position relative to the main window
                this.Left = mainWindow.Left + (mainWindow.Width - this.Width) / 2;
                this.Top = mainWindow.Top + (mainWindow.Height - this.Height) / 2;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void SignUpLink_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            // Create the SignUpViewModel and pass the UserRepository
            var signUpViewModel = new SignUpViewModel(_userRepository, this);

            SignUpWindow signUpWindow = new SignUpWindow(signUpViewModel);
            signUpWindow.DataContext = signUpViewModel; // Bind the ViewModel to the Window's DataContext

            signUpWindow.ShowDialog();

        }   

        private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordText = ((PasswordBox)sender).Password;

            if (!string.IsNullOrEmpty(PasswordText))
            {
                PasswordPlaceholderText.Text = "";
            }
            else
            {
                PasswordPlaceholderText.Text = "Enter your password";
            }
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordField.Password;
            // Check if the password field is empty
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.");
                PasswordField.Focus();  // Set focus to the password field
                return; // Stop further execution
            }

            // Check if the username field is empty
            if (string.IsNullOrEmpty(UsernameField.Text))
            {
                MessageBox.Show("Please enter your username.");
                UsernameField.Focus();  // Set focus to the username field
                return; // Stop further execution
            }
        }
    }
}
