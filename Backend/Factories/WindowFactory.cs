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

        private readonly Func<SignInViewModel> _createSignInViewModel;

        public WindowFactory(Func<SignUpViewModel> createSignUpViewModel, Func<SignInViewModel> createSignInViewModel, IServiceProvider serviceProvider)
        {
            _createSignInViewModel = createSignInViewModel;
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

        public SignInViewModel CreateSignInViewModel()
        {
            return _serviceProvider.GetRequiredService<SignInViewModel>();
        }

        public Window CreateSignInWindow()
        {
            return new SignInWindow(_createSignInViewModel());
        }

    }

}
