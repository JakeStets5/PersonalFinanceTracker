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
using System.Windows;
using System.Windows.Input;

namespace PersonalFinanceTracker.ViewModels
{
    public class UploadTransactionViewModel : BindableBase
    {
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

        private string _incomeDate;
        public string IncomeDate
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

        private string _expenseDate;
        public string ExpenseDate
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

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public ObservableCollection<string> IncomeTemplates { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ExpenseTemplates { get; set; } = new ObservableCollection<string>();

        public ICommand AddIncomeStatementCommand { get; }
        public ICommand SaveIncomeTemplateCommand { get; }
        public ICommand AddExpenseStatementCommand { get; }
        public ICommand SaveExpenseTemplateCommand { get; }

        private readonly IAwsDynamoDbService _dynamoDbService;

        private readonly IUserSessionService _userSessionService;   

        public UploadTransactionViewModel(IAwsDynamoDbService dbService, IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
            _dynamoDbService = dbService;
            AddIncomeStatementCommand = new RelayCommand(AddIncomeStatement);
            SaveIncomeTemplateCommand = new RelayCommand(SaveIncomeTemplate);
            AddExpenseStatementCommand = new RelayCommand(AddExpenseStatement);
            SaveExpenseTemplateCommand = new RelayCommand(SaveExpenseTemplate);
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

        private bool ValidateIncomeStatement(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(IncomeSource) || string.IsNullOrWhiteSpace(IncomeFrequency))
            {
                errorMessage = "Income source and frequency are required.";
                return false;
            }

            if (!decimal.TryParse(IncomeAmount, out decimal amount) || amount <= 0)
            {
                errorMessage = "Invalid income amount.";
                return false;
            }

            if (!DateTime.TryParse(IncomeDate, out _))
            {
                errorMessage = "Invalid date format.";
                return false;
            }

            errorMessage = null;
            return true;
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
