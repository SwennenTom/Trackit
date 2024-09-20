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
        private bool _straight;
        private bool _splines;
        private bool _stepped;
        private bool _scatter;
        private bool _showMinThreshold;
        private bool _showMaxThreshold;
        private bool _showTrendLine;

        public string MinThreshold { get; set; }
        public string MaxThreshold { get; set; }

        public bool Straight
        {
            get => _straight;
            set
            {
                if (_straight == value) return;
                _straight = value;
                if (_straight)
                {
                    _splines = false;
                    _stepped = false;
                    OnPropertyChanged(nameof(Splines));
                    OnPropertyChanged(nameof(Stepped));
                }
                OnPropertyChanged();

            }
        }

        public bool Splines
        {
            get => _splines;
            set
            {
                if (_splines == value) return;
                _splines = value;
                if (_splines)
                {
                    _straight = false;
                    _stepped = false;
                    OnPropertyChanged(nameof(Straight));
                    OnPropertyChanged(nameof(Stepped));
                }
                OnPropertyChanged();

            }
        }

        public bool Stepped
        {
            get => _stepped;
            set
            {
                if (_stepped == value) return;
                _stepped = value;
                if (_stepped)
                {
                    _straight = false;
                    _splines = false;
                    OnPropertyChanged(nameof(Straight));
                    OnPropertyChanged(nameof(Splines));
                }
                OnPropertyChanged();

            }
        }

        public bool Scatter
        {
            get => _scatter;
            set
            {
                _scatter = value;
                OnPropertyChanged();

            }
        }

        public bool ShowMinThreshold
        {
            get => _showMinThreshold;
            set
            {
                _showMinThreshold = value;
                OnPropertyChanged();

            }
        }

        public bool ShowMaxThreshold
        {
            get => _showMaxThreshold;
            set
            {
                _showMaxThreshold = value;
                OnPropertyChanged();

            }
        }

        public bool ShowTrendLine
        {
            get => _showTrendLine;
            set
            {
                _showTrendLine = value;
                OnPropertyChanged();

            }
        }
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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "TrackerSettings cannot be null");
            }

            _settings = settings;
            MinThreshold = settings.min_threshhold.ToString();
            MaxThreshold = settings.max_threshold.ToString();
            Straight = settings.straight;
            Splines = settings.splines;
            Stepped = settings.stepped;
            Scatter = settings.scatter;
            ShowMinThreshold = settings.showMinThreshold;
            ShowMaxThreshold = settings.showMaxThreshold;
            ShowTrendLine = settings.showTrendLine;
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
                _settings.showMinThreshold = ShowMinThreshold;
                _settings.showMaxThreshold = ShowMaxThreshold;
                _settings.showTrendLine = ShowTrendLine;

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
