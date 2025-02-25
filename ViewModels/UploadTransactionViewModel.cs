using PersonalFinanceTracker.Backend.Commands;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Backend.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Input;
using Microsoft.AspNetCore.Hosting.Server;
using PersonalFinanceTracker.Backend.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class UploadTransactionViewModel : BindableBase
    {
       
        #region I/E Fields

        private string _incomeAmount;
        public string IncomeAmount
        {
            get => _incomeAmount;
            set => SetProperty(ref _incomeAmount, value);
        }

        private string _expenseAmount;
        public string ExpenseAmount
        {
            get => _expenseAmount;
            set => SetProperty(ref _expenseAmount, value);
        }

        private string _incomeSource;
        public string IncomeSource
        {
            get => _incomeSource;
            set => SetProperty(ref _incomeSource, value);
        }

        private string _incomeFrequency;
        public string IncomeFrequency
        {
            get => _incomeFrequency;
            set => SetProperty(ref _incomeFrequency, value);
        }

        private DateTime _incomeDate;
        public DateTime IncomeDate
        {
            get => _incomeDate;
            set => SetProperty(ref _incomeDate, value);
        }

        private string _incomePaymentMethod;
        public string IncomePaymentMethod
        {
            get => _incomePaymentMethod;
            set => SetProperty(ref _incomePaymentMethod, value);
        }

        private string _expenseSource;
        public string ExpenseSource
        {
            get => _expenseSource;
            set => SetProperty(ref _expenseSource, value);
        }

        private string _expenseFrequency;
        public string ExpenseFrequency
        {
            get => _expenseFrequency;
            set => SetProperty(ref _expenseFrequency, value);
        }

        private DateTime _expenseDate;
        public DateTime ExpenseDate
        {
            get => _expenseDate;
            set => SetProperty(ref _expenseDate, value);
        }

        private string _expensePaymentMethod;
        public string ExpensePaymentMethod
        {
            get => _expensePaymentMethod;
            set => SetProperty(ref _expensePaymentMethod, value);
        }
        #endregion

        private bool _isUserSignedIn;
        public bool IsUserSignedIn
        {
            get => _isUserSignedIn;
            set => SetProperty(ref _isUserSignedIn, value);
        }

        private SeriesCollection _incomeSeries;
        public SeriesCollection IncomeSeries
        {
            get => _incomeSeries;
            set
            {
                _incomeSeries = value;
                OnPropertyChanged(nameof(IncomeSeries));
            }
        }

        private SeriesCollection _expenseSeries;
        public SeriesCollection ExpenseSeries
        {
            get => _expenseSeries;
            set
            {
                _expenseSeries = value;
                OnPropertyChanged(nameof(ExpenseSeries));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Dropdown Options
        public ObservableCollection<string> IncomeSourceOptions { get; set; } = new ObservableCollection<string>
        {
            "Salary",
            "Side Job",
            "Investment",
            "Gift",
            "Rental Income",
            "Other"
        };

        public ObservableCollection<string> IncomeFrequencyOptions { get; set; } = new ObservableCollection<string>
        {
            "One-Time",
            "Daily",
            "Weekly",
            "Bi-Weekly",
            "Monthly",
            "Annually",
            "Other"
        };

        public ObservableCollection<string> IncomePaymentMethodOptions { get; set; } = new ObservableCollection<string>
        {
            "Cash",
            "Credit Card",
            "Debit Card",
            "Check",
            "Bank Transfer",
            "Third Party App",
            "Other"
        };

        public ObservableCollection<string> ExpenseSourceOptions { get; set; } = new ObservableCollection<string>
        {
            "Rent/Utilities/Maintinance",
            "Groceries & Dining",
            "Banking, Investments, & Loans",
            "Transportation",
            "Entertainment",
            "Healthcare",
            "Shopping",
            "Education",
            "Gifts & Donations",
            "Other"
        };

        public ObservableCollection<string> ExpenseFrequencyOptions { get; set; } = new ObservableCollection<string>
        {
            "One-Time",
            "Daily",
            "Weekly",
            "Bi-Weekly",
            "Monthly",
            "Annually",
            "Other"
        };

        public ObservableCollection<string> ExpensePaymentMethodOptions { get; set; } = new ObservableCollection<string>
        {
            "Cash",
            "Credit Card",
            "Debit Card",
            "Check",
            "Bank Transfer",
            "Third Party App",
            "Other"
        };
        #endregion

        public ObservableCollection<string> IncomeTemplates { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ExpenseTemplates { get; set; } = new ObservableCollection<string>();

        public ICommand AddIncomeStatementCommand { get; }
        public ICommand SaveIncomeTemplateCommand { get; }
        public ICommand AddExpenseStatementCommand { get; }
        public ICommand SaveExpenseTemplateCommand { get; }
        public ICommand SignInCommand { get; }

        private readonly IAwsDynamoDbService _dynamoDbService;
        private readonly IUserSessionService _userSessionService;
        private readonly IFinancialDataService _financialDataService;
        private readonly INavigationService _navigationService;


        public UploadTransactionViewModel(IAwsDynamoDbService dbService, IUserSessionService userSessionService, IFinancialDataService financialDataService, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _financialDataService = financialDataService;   
            _userSessionService = userSessionService;
            _dynamoDbService = dbService;

            AddIncomeStatementCommand = new RelayCommand(AddIncomeStatement);
            SaveIncomeTemplateCommand = new RelayCommand(SaveIncomeTemplate);
            AddExpenseStatementCommand = new RelayCommand(AddExpenseStatement);
            SaveExpenseTemplateCommand = new RelayCommand(SaveExpenseTemplate);
            SignInCommand = new RelayCommand(SignIn);

            LoadFinancialDataAsync();
        }

        private async void AddIncomeStatement()
        {
            if(!_userSessionService.IsUserLoggedIn)
            {
                MessageBox.Show("You must be signed in to add a statement.");
                return;
            }

            var statement = new Statement
            {
                UserId = _userSessionService.UserId,
                StatementId = Guid.NewGuid().ToString(),
                Type = "Income",
                Amount = decimal.Parse(IncomeAmount),
                Source = IncomeSource,
                Frequency = IncomeFrequency,
                Date = IncomeDate,
                PaymentMethod = IncomePaymentMethod
            };

            try
            {
                await _dynamoDbService.SaveStatementAsync(statement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving income statement: {ex.Message}");
            }
        }

        private void SignIn()
        {
            _navigationService.OpenSignInWindow();
        }

        private void SaveIncomeTemplate()
        {
            // Logic to save an income template
        }

        private async void AddExpenseStatement()
        {
            if (!_userSessionService.IsUserLoggedIn)
            {
                MessageBox.Show("You must be signed in to add a statement.");
                return;
            }

            var statement = new Statement
            {
                UserId = _userSessionService.UserId,
                StatementId = Guid.NewGuid().ToString(),
                Type = "Expense",
                Amount = decimal.Parse(ExpenseAmount),
                Source = ExpenseSource,
                Frequency = ExpenseFrequency,
                Date = ExpenseDate,
                PaymentMethod = ExpensePaymentMethod
            };

            try
            {
                await _dynamoDbService.SaveStatementAsync(statement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving income statement: {ex.Message}");
            }
        }

        private void SaveExpenseTemplate()
        {
            // Logic to save an expense template
        }

        private async Task LoadFinancialDataAsync()
        {
            if (!_userSessionService.IsUserLoggedIn)
            {
                IsUserSignedIn = false;
                return;
            }

            var userId = _userSessionService.UserId;
            var breakdown = await _financialDataService.GetFinancialBreakdownAsync(userId);

            IncomeSeries = new SeriesCollection(
                breakdown.IncomeByCategory.Select(kvp =>
                new PieSeries
                {
                    Title = kvp.Key,
                    Values = new ChartValues<decimal> { kvp.Value }
                })
            );

            ExpenseSeries = new SeriesCollection(
                breakdown.ExpensesByCategory.Select(kvp =>
                    new PieSeries
                    {
                        Title = kvp.Key,
                        Values = new ChartValues<decimal> { kvp.Value }
                    })
            );


            IsUserSignedIn = true;
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
            return true;
        }
    }
}
