using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalFinanceTracker.Backend.Services
{
    public class NavigationService: INavigationService
    {
        private readonly IWindowFactory _windowFactory;
        private readonly IRegionManager _regionManager;

        public NavigationService(IWindowFactory windowFactory, IRegionManager regionManager)
        {
            _windowFactory = windowFactory;
            _regionManager = regionManager; 
        }

        public void Navigate(string regionName, string viewName)
        {
            if (string.IsNullOrWhiteSpace(regionName) || string.IsNullOrWhiteSpace(viewName))
                throw new ArgumentException("Region name and view name cannot be null or empty.");

            _regionManager.RequestNavigate("MainRegion", nameof(UploadTransactionView), navigationResult =>
            {
                if (navigationResult != null && !navigationResult.Success)
                {
                    var errorMessage = navigationResult.Exception != null
                        ? navigationResult.Exception.Message
                        : "Unknown navigation error.";
                    MessageBox.Show($"Navigation failed: {errorMessage}");
                }
            });
        }

        public void OpenSignUpWindow()
        {
            var signUpWindow = _windowFactory.CreateSignUpWindow();
            signUpWindow.Show();
        }
    }

}
