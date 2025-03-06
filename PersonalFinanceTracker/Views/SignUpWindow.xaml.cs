using PersonalFinanceTracker.Common.Models;
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
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker.Views
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow(SignUpViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Subscribe to the completion event
            viewModel.OnSignUpCompleted += ViewModel_OnSignUpCompleted;

            // Set window position relative to the main window
            CenterToMainWindow();
        }

        // Close the window when sign-up is completed
        public void ViewModel_OnSignUpCompleted()
        {
            this.Close();
        }

        // Centers the sign up window in the main window
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
