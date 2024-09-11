using System.ComponentModel;
using System.Windows.Input;
using Trackit.Models;

namespace Trackit.ViewModels
{
    public class CreateTrackerViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MinThreshold { get; set; }
        public string MaxThreshold { get; set; }

        public ICommand CreateTrackerCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public CreateTrackerViewModel()
        {
            // Initialize the command for creating a tracker
            CreateTrackerCommand = new Command(OnCreateTracker);
        }

        private async void OnCreateTracker()
        {
            // Convert threshold inputs to integers
            bool isMinValid = int.TryParse(MinThreshold, out int minThreshold);
            bool isMaxValid = int.TryParse(MaxThreshold, out int maxThreshold);

            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) || !isMinValid || !isMaxValid)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please fill out all fields correctly.", "OK");
                return;
            }

            // Create new Tracker object
            var newTracker = new Tracker
            {
                name = Name,
                description = Description
            };

            // Create new TrackerSettings object with the thresholds
            var trackerSettings = new TrackerSettings
            {
                min_threshhold = minThreshold,
                max_threshold = maxThreshold,
                tracker_id = newTracker.tracker_id
            };

            IsBusy = true;
            // Save Tracker to database with the linked SettingsId
            await App.Database.AddTrackerAsync(newTracker);

            // Save TrackerSettings to database
            await App.Database.AddSettingsAsync(trackerSettings);

            IsBusy = false;

            // Navigate back to Home page or refresh the list
            await App.Current.MainPage.Navigation.PopAsync();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
