using System;
using System.Windows.Input;
using Trackit.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trackit.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        private int _trackerId;
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private DateTime _fromDate = DateTime.Now.AddDays(-1);
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _toDate = DateTime.Now;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExportCommand { get; }

        public ExportViewModel(int trackerId)
        {
            ExportCommand = new Command(OnExport);
            _trackerId = trackerId;
        }

        private async void OnExport()
        {
            // Logic for generating the Excel report and opening email client
            // For now, display a simple message as a placeholder
            await App.Current.MainPage.DisplayAlert("Export", "The report will be generated and sent via email.", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
