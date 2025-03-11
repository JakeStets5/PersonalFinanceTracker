using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
