using PersonalFinanceTracker.ViewModels;
using PersonalFinanceTracker.Views;
using PersonalFinanceTracker.Backend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PersonalFinanceTracker.Backend.Services
{
    public class PFTDialogService : IPFTDialogService
    {
        public void ShowSuccessMessage(string message)
        {
            var successWindow = new SuccessWindow(new SuccessPopUpViewModel(message));
            successWindow.Show();  // Show the success window

            // Create a timer to close the success window after 2 seconds
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            timer.Tick += (sender, args) =>
            {
                successWindow.Close();  // Close the pop-up window after 2 seconds
                timer.Stop();  // Stop the timer
            };

            timer.Start();
        }
    }
}
