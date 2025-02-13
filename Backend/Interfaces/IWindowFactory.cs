using PersonalFinanceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalFinanceTracker.Backend.Interfaces
{
    public interface IWindowFactory
    {
        Window CreateSignUpWindow();

        SignUpViewModel CreateSignUpViewModel();
    }
}
