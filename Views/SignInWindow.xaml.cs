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
    public partial class SignInWindow : Window
    {

        private readonly UserRepository _userRepository;

        public SignInWindow(UserRepository userRepository, SignInViewModel signInViewModel)
        {
            InitializeComponent();
            _userRepository = userRepository;

            // Set window position relative to the main window
            CenterToMainWindow();

            DataContext = signInViewModel;
            signInViewModel.CloseAction = Close;
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
    }
}
