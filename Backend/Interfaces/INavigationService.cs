using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Interfaces
{
    public interface INavigationService
    {
        void OpenSignUpWindow();

        void Navigate(string regionName, string viewName);
        //void CloseSignIn();
    }
}
