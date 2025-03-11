using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace PersonalFinanceTracker.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        private readonly INavigationService _navigationService;
        private readonly IRegionManager _regionManager;
        public ICommand UploadTransactionCommand { get; private set;  }
        public ICommand SignInCommand { get; private set; }

        public DelegateCommand<string> NavigateCommand { get; }

        public MainWindowViewModel(INavigationService navigationService, IRegionManager regionManager)
        {
            _navigationService = navigationService;
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);

            UploadTransactionCommand = new DelegateCommand(OnUploadTransaction);
            SignInCommand = new DelegateCommand(SignIn);
        }

        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                _regionManager.RequestNavigate("MainRegion", viewName);
            }
        }

        private void SignIn()
        {
            _navigationService.OpenSignInWindow();
        }

        private void OnUploadTransaction()
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (_regionManager.Regions.ContainsRegionWithName("MainRegion"))
                {
                    _regionManager.RequestNavigate("MainRegion", "UploadTransactionView");
                }
                else
                {
                    Debug.WriteLine("MainRegion is still not registered!");
                }
            });
        }
    }
}
