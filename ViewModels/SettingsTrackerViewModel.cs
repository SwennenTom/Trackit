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
        public string MinThreshold { get; set; }
        public string MaxThreshold { get; set; }
        public bool Straight { get; set; }
        public bool Splines { get; set; }
        public bool Stepped { get; set; }
        public bool Scatter { get; set; }
        public bool MarkArea { get; set; }
        public bool ShowMinThreshold { get; set; }
        public bool ShowMaxThreshold { get; set; }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public SettingsTrackerViewModel(TrackerSettings settings)
        {
            _settings = settings;
            MinThreshold = settings.min_threshhold.ToString();
            MaxThreshold = settings.max_threshold.ToString();
            Straight = settings.straight;
            Splines = settings.splines;
            Stepped = settings.stepped;
            Scatter = settings.scatter;
            MarkArea = settings.markArea;
            ShowMinThreshold = settings.showMinThreshold;
            ShowMaxThreshold = settings.showMaxThreshold;
            SaveCommand = new Command(async () => await SaveSettingsAsync());
        }

        public ICommand SaveCommand { get; }

        private async Task SaveSettingsAsync()
        {
            if (double.TryParse(MinThreshold, out double minThreshold) &&
                double.TryParse(MaxThreshold, out double maxThreshold))
            {
                _settings.min_threshhold = minThreshold;
                _settings.max_threshold = maxThreshold;

                _settings.straight = Straight;
                _settings.splines = Splines;
                _settings.stepped = Stepped;
                _settings.scatter = Scatter;
                _settings.markArea = MarkArea;
                _settings.showMinThreshold = ShowMinThreshold;
                _settings.showMaxThreshold = ShowMaxThreshold;

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
