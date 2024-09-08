using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Trackit.Models;

namespace Trackit.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Tracker> Trackers { get; set; }
        public ICommand AddTrackerCommand { get; }
        public ICommand LongPressCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public HomeViewModel()
        {
            Trackers = new ObservableCollection<Tracker>();

            AddTrackerCommand = new Command(OnAddTracker);
            LongPressCommand = new Command<Tracker>(OnLongPress);

        }

        public async void LoadTrackers()
        {

            Trackers.Clear();
            var trackersFromDb = await App.Database.GetTrackersAsync();

            foreach(var tracker in trackersFromDb)
            {
                Trackers.Add(tracker);
            }
        }

        private async void OnAddTracker()
        {
            await App.Current.MainPage.Navigation.PushAsync(new Trackit.Screens.CreateTracker());
        }


        private async void OnLongPress(Tracker tracker)
        {
            bool result = await App.Current.MainPage.DisplayAlert("Delete Tracker", $"Are you sure you want to delete {tracker.name}?", "Yes", "No");

            if(result)
            {
                Trackers.Remove(tracker);
                await App.Database.DeleteTrackerAsync(tracker);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
