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
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker.Views
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow(SignUpViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
