using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for SuccessWindow.xaml
    /// </summary>
    public partial class SuccessWindow : Window
    {
        public SuccessWindow()
        {
            InitializeComponent();

            // Set window position relative to the main window
            CenterToMainWindow();
        }

        // Centers the success window in the main window
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
