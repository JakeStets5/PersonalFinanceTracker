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

        public NavigationService(IWindowFactory windowFactory)
        {
            _windowFactory = windowFactory;
        }

        public void OpenSignUpWindow()
        {
            var signUpWindow = _windowFactory.CreateSignUpWindow();
            signUpWindow.Show();
        }
    }

}
