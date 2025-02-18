using PersonalFinanceTracker.Backend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        private readonly INavigationService _navigationService;
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; }

        public MainWindowViewModel(INavigationService navigationService, IRegionManager regionManager)
        {
            _navigationService = navigationService;
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                _regionManager.RequestNavigate("MainRegion", viewName);
            }
        }
    }
}
