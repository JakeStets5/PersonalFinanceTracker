using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.ViewModels;
using PersonalFinanceTracker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalFinanceTracker.Backend.Factories
{
    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Func<SignUpViewModel> _createSignUpViewModel;

        public WindowFactory(Func<SignUpViewModel> createSignUpViewModel, IServiceProvider serviceProvider)
        {
            _createSignUpViewModel = createSignUpViewModel;
            _serviceProvider = serviceProvider;
        }

        public SignUpViewModel CreateSignUpViewModel()
        {
            return _serviceProvider.GetRequiredService<SignUpViewModel>();
        }

        public Window CreateSignUpWindow()
        {
            return new SignUpWindow(_createSignUpViewModel());
        }
    }

}
