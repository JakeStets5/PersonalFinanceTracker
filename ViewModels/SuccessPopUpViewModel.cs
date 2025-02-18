using GalaSoft.MvvmLight;
using PersonalFinanceTracker.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.ViewModels
{
    public class SuccessPopUpViewModel : ViewModelBase
    {
        private string _successMessage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }
            
        public SuccessPopUpViewModel(string message)
        {
            SuccessMessage = message;
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
