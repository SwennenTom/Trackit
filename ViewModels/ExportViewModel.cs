using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.XlsIO;
using Trackit.Models;


namespace Trackit.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        #region Init
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
        #endregion
        #region Commands
        public ICommand ExportCommand { get; }
        public ICommand LastWeekCommand { get; }
        public ICommand LastMonthCommand { get; }
        public ICommand LastYearCommand { get; }
        public ICommand AllTimeCommand { get; }

        #endregion

        public ExportViewModel(int trackerId)
        {
            _trackerId = trackerId;

            LastWeekCommand = new Command(SetLastWeek);
            LastMonthCommand = new Command(SetLastMonth);
            LastYearCommand = new Command(SetLastYear);
            AllTimeCommand = new Command(async () => await SetAllTimeAsync());
            ExportCommand = new Command(OnExport);
            
        }

        #region Set Dates
        private void SetLastWeek()
        {
            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now;
        }

        private void SetLastMonth()
        {
            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;
        }

        private void SetLastYear()
        {
            FromDate = DateTime.Now.AddYears(-1);
            ToDate = DateTime.Now;
        }

        private async Task SetAllTimeAsync()
        {
            var earliestDate = await GetEarliestDateFromDatabaseAsync();
            FromDate = earliestDate ?? DateTime.Now;
            ToDate = DateTime.Now;
            ToDate = DateTime.Now;
        }

        private async Task<DateTime?> GetEarliestDateFromDatabaseAsync()
        {

            var allValues = await App.Database.GetValuesForTrackerAsync(_trackerId);
            return allValues.Min(v => v.date);
        }

        #endregion

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
