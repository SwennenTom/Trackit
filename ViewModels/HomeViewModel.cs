using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Trackit.Models;
using Trackit.Screens;
using System.Runtime.CompilerServices;

namespace Trackit.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Tracker> Trackers { get; set; }
        public ICommand AddTrackerCommand { get; }
        public ICommand OnSwipeCommand { get; }
        public ICommand NavigateToDetailCommand { get; }
        public ICommand ShowHomeInfoCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Tracker _tracker;
        public string Name => _tracker.name;

        private readonly INavigation _navigation;

        public HomeViewModel(INavigation navigation)
        {
            Trackers = new ObservableCollection<Tracker>();

            AddTrackerCommand = new Command(OnAddTracker);
            OnSwipeCommand = new Command<Tracker>(OnSwipe);
            NavigateToDetailCommand = new Command<Tracker>(OnNavigateToDetail);
            ShowHomeInfoCommand = new Command(OnShowHomeInfo);
            _navigation = navigation;
        }

        public async void LoadTrackers()
        {

            Trackers.Clear();
            var trackersFromDb = await App.Database.GetTrackersAsync();

            foreach (var tracker in trackersFromDb)
            {
                Trackers.Add(tracker);
            }
        }

        private async void OnAddTracker()
        {
            await _navigation.PushAsync(new CreateTracker());
        }


        private async void OnSwipe(Tracker tracker)
        {
                bool result = await App.Current.MainPage.DisplayAlert("Delete Tracker", $"Are you sure you want to delete {tracker.name}?", "Yes", "No");

                if (result)
                {
                    Trackers.Remove(tracker);
                    await App.Database.DeleteTrackerAsync(tracker.tracker_id);
                }
        }
        private async void OnNavigateToDetail(Tracker tracker)
        {
            if (tracker != null)
            {
                // Make sure the tracker object and its ID are valid
                await _navigation.PushAsync(new Detail(tracker));
            }
            else
            {
                // Handle invalid tracker case
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid tracker selected", "OK");
            }
        }


        private async void OnShowHomeInfo()
        {
            await App.Current.MainPage.DisplayAlert("Info","Tap + to create a new tracker. Tap on an existing tracker to view the graph. Double tap on an existing tracker to delete it.", "Ok");
        }



        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
