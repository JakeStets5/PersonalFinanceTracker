using PersonalFinanceTracker.Backend.Commands;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Common.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;

namespace PersonalFinanceTracker.ViewModels
{
    public class UploadTransactionViewModel : BindableBase, IDisposable, INotifyPropertyChanged
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

        private DateTime _incomeDate = DateTime.Now;
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

        private DateTime _expenseDate = DateTime.Now;
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

        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value) // Prevent redundant updates
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                    LoadStatementsAsync(); // Refresh chart when date changes
                }
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                LoadStatementsAsync(); // Refresh chart when date changes
            }
        }

        private bool _isUserSignedIn;
        public bool IsUserSignedIn
        {
            get => _isUserSignedIn;
            set => SetProperty(ref _isUserSignedIn, value);
        }

        private User? _currentUser;

        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                SetProperty(ref _currentUser, value);
                OnPropertyChanged(nameof(IsUserSignedIn));
            }
        }

        // Populates the income pie charts
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

        // Populates the expense pie charts
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

        private List<SolidColorBrush> GetChartColors() => new()
        {
            (SolidColorBrush)new BrushConverter().ConvertFrom("#0F4C5C"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#10375C"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#136F63"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#5D3754"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#3E3A66"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#1B3A4B"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#2C497F"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#4E5166"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#6B2737"),
            (SolidColorBrush)new BrushConverter().ConvertFrom("#264653")
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> IncomeTemplates { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ExpenseTemplates { get; set; } = new ObservableCollection<string>();

        public ICommand AddIncomeStatementCommand { get; } // Triggered on AddIncomeStatement button click
        public ICommand SaveIncomeTemplateCommand { get; } // Triggered on SaveIncomeTemplateCommand button click
        public ICommand AddExpenseStatementCommand { get; } // Triggered on AddExpenseStatementCommand button click
        public ICommand SaveExpenseTemplateCommand { get; } // Triggered on SaveExpenseTemplateCommand button click
        public ICommand SignInCommand { get; } // Triggered on SignInCommand button click
        public ICommand RefreshCommand { get; } // Triggered on RefreshCommand button click

        private readonly IUserSessionService _userSessionService; // Keeps the user signed in 
        private readonly INavigationService _navigationService; // Handles navigation between windows. Including pop-ups
        private readonly IApiClient _apiClient; // Handles the API connection 

        public UploadTransactionViewModel(IUserSessionService userSessionService,INavigationService navigationService, IApiClient apiClient)
        {
            _apiClient = apiClient;
            _navigationService = navigationService;   
            _userSessionService = userSessionService;

            AddIncomeStatementCommand = new RelayCommand(AddIncomeStatement);
            SaveIncomeTemplateCommand = new RelayCommand(SaveIncomeTemplate);
            AddExpenseStatementCommand = new RelayCommand(AddExpenseStatement);
            SaveExpenseTemplateCommand = new RelayCommand(SaveExpenseTemplate);
            SignInCommand = new RelayCommand(SignIn);
            RefreshCommand = new RelayCommand(Refresh);

            _userSessionService.UserSignedIn += OnUserSignedIn; // Subscribes to the user sign in event. Starts API call

            _startDate = DateTime.Today.AddMonths(-1); // Default: One month ago
            _endDate = DateTime.Today;
        }

        private async void OnUserSignedIn(User? user)
        {
            IsUserSignedIn = true;
            CurrentUser = user;
            await LoadStatementsAsync();// Begins the API call to query cloud db
        }
        public void Dispose()
        {
            _userSessionService.UserSignedIn -= OnUserSignedIn;
            GC.SuppressFinalize(this);
        }

        private async void Refresh()
        {
            await LoadStatementsAsync();
        }

        // Starts the call to add an income statement to the API
        private async void AddIncomeStatement()
        {
            // Verifies user login
            if(!_userSessionService.IsUserLoggedIn)
            {
                MessageBox.Show("You must be signed in to add a statement.");
                return;
            }

            var statement = new Statement
            {
                UserId = _userSessionService.UserId, // Keeping userId(partition key) consistent across all statements saved
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
                var result = await _apiClient.SubmitStatementAsync(statement); // 
                if (result != null)
                {
                    await LoadStatementsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving income statement: {ex.Message}");
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
                var result = await _apiClient.SubmitStatementAsync(statement);
                if (result != null)
                {
                    await LoadStatementsAsync();
                }
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

        private SeriesCollection GenerateSeriesCollection(Dictionary<string, decimal> data)
        {
            var colors = GetChartColors();
            var series = new SeriesCollection();

            foreach (var kvp in data)
            {
                series.Add(new PieSeries
                {
                    Title = kvp.Key,
                    Values = new ChartValues<decimal> { kvp.Value },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y:C}",
                    Fill = colors[data.Keys.ToList().IndexOf(kvp.Key) % colors.Count] // Safe index
                });
            }

            return series;
        }

        private async Task LoadStatementsAsync()
        {
            // Verifies user login
            if (!_userSessionService.IsUserLoggedIn)
            {
                IsUserSignedIn = false;
                return;
            }

            var userId = _userSessionService.UserId;

            // Calls the client (object holding the connection) to begin cosmos (or cloud provider) statement query by userId.
            // Matches stored in statements
            var statements = await _apiClient.GetStatementsAsync(userId, StartDate, EndDate);
            if (statements == null || !statements.Any())
            {
                Debug.WriteLine("No statements to process.");
                IncomeSeries.Clear();
                ExpenseSeries.Clear();
                return;
            }

            // Filter and group income statements by Source + sum Amounts
            var incomeData = statements
                .Where(s => s.Type == "Income")
                .GroupBy(s => s.Source)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));

            // Filter and group expense statements by Source + sum Amounts
            var expenseData = statements
                .Where(s => s.Type == "Expense")
                .GroupBy(s => s.Source)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));

            // Update pie charts—generate series for each.
            IncomeSeries = GenerateSeriesCollection(incomeData);
            OnPropertyChanged(nameof(IncomeSeries));

            ExpenseSeries = GenerateSeriesCollection(expenseData);
            OnPropertyChanged(nameof(ExpenseSeries));

            IsUserSignedIn = true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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
