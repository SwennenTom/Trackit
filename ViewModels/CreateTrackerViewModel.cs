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
            try
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

                IsBusy = true;
                int trackerId = await App.Database.AddTrackerAsync(newTracker);

                if (trackerId <= 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to save the tracker.", "OK");
                    IsBusy = false;
                    return;
                }

                // Fetch the newly created tracker to get the correct trackerId
                var savedTracker = await App.Database.GetTrackerByNameAsync(Name);

                if (savedTracker == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to retrieve the newly created tracker.", "OK");
                    IsBusy = false;
                    return;
                }

                var trackerSettings = new TrackerSettings
                {
                    min_threshhold = minThreshold,
                    max_threshold = maxThreshold,
                    tracker_id = savedTracker.tracker_id
                };

                // Save TrackerSettings to database
                int result = await App.Database.AddSettingsAsync(trackerSettings);

                if (result <= 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to save settings.", "OK");
                }

                // Navigate back to Home page or refresh the list
                await App.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error creating tracker: {ex.Message}");

                // Display an error message to the user
                await App.Current.MainPage.DisplayAlert("Error", "An error occurred while creating the tracker. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
