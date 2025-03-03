using PersonalFinanceTracker.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonalFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for UploadTransactionView.xaml
    /// </summary>
    public partial class UploadTransactionView : UserControl
    {
        public UploadTransactionView(UploadTransactionViewModel uploadTransactionViewModel)
        {
            InitializeComponent();
            DataContext = uploadTransactionViewModel;
            this.Unloaded += (s, e) => uploadTransactionViewModel.Dispose();
        }

        #region // Income Amount Field (Formatting)
        private void IncomeAmountField_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(IncomeAmountField.Text, out decimal amount))
            {
                IncomeAmountField.Text = amount.ToString("N2"); // Format as currency with two decimal places
            }
            else
            {
                IncomeAmountField.Text = "0.00"; // Fallback for invalid input
            }
        }

        private void IncomeAmountField_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits and a single dot
            e.Handled = !IsTextAllowed(e.Text, (sender as TextBox).Text);
        }

        private bool IsTextAllowed(string newText, string currentText)
        {
            // Check if the newText is a digit or a dot
            if (decimal.TryParse(currentText + newText, out _))
                return true;

            return false;
        }
        #endregion

        #region // Expense Amount Field (Formatting)
        private void ExpenseAmountField_LostFocus(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(ExpenseAmountField.Text, out decimal amount))
            {
                ExpenseAmountField.Text = amount.ToString("N2"); // Format as currency with two decimal places
            }
            else
            {
                ExpenseAmountField.Text = "0.00"; // Fallback for invalid input
            }
        }

        private void ExpenseAmountField_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits and a single dot
            e.Handled = !IsTextAllowed(e.Text, (sender as TextBox).Text);
        }
        #endregion
    }
}
