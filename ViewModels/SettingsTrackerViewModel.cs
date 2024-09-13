using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using Trackit;
using Trackit.Models;
using Trackit.ViewModels;   

namespace Trackit.ViewModels
{
    public class SettingsTrackerViewModel : BindableObject
    {
        private TrackerSettings _settings;
        private bool _isBusy;

        public SettingsTrackerViewModel(TrackerSettings settings)
        {
            _settings = settings;
            MinThreshold = settings.min_threshhold.ToString();
            MaxThreshold = settings.max_threshold.ToString();
            SaveCommand = new Command(async () => await SaveSettingsAsync());
        }

        public string MinThreshold { get; set; }
        public string MaxThreshold { get; set; }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        private async Task SaveSettingsAsync()
        {
            if (double.TryParse(MinThreshold, out double minThreshold) &&
                double.TryParse(MaxThreshold, out double maxThreshold))
            {
                _settings.min_threshhold = minThreshold;
                _settings.max_threshold = maxThreshold;

                IsBusy = true;
                await App.Database.UpdateSettingsAsync(_settings);
                IsBusy = false;
            }
            else
            {
                // Handle invalid input
            }
        }
    }
}
