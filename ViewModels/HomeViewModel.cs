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
        public ICommand OnDeleteCommand { get; }
        public ICommand NavigateToDetailCommand { get; }
        public ICommand ShowHomeInfoCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Tracker _tracker;
        public string Name => _tracker.name;

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

        private readonly INavigation _navigation;

        public HomeViewModel(INavigation navigation)
        {
            Trackers = new ObservableCollection<Tracker>();

            AddTrackerCommand = new Command(OnAddTracker);
            OnDeleteCommand = new Command<Tracker>(OnDelete);
            NavigateToDetailCommand = new Command<Tracker>(OnNavigateToDetail);
            ShowHomeInfoCommand = new Command(OnShowHomeInfo);

            _navigation = navigation;
        }

        public async void LoadTrackers()
        {
            IsBusy = true;
            Trackers.Clear();
            var trackersFromDb = await App.Database.GetTrackersAsync();

            foreach (var tracker in trackersFromDb)
            {
                Trackers.Add(tracker);
            }
            IsBusy = false;
        }

        private async void OnAddTracker()
        {
            await _navigation.PushAsync(new CreateTracker());
        }


        private async void OnDelete(Tracker tracker)
        {
                bool result = await App.Current.MainPage.DisplayAlert("Delete Tracker", $"Are you sure you want to delete {tracker.name}?", "Yes", "No");

                if (result)
                {
                    IsBusy = true;
                    Trackers.Remove(tracker);
                    await App.Database.DeleteTrackerAsync(tracker.tracker_id);
                    IsBusy = false;
                }
        }
        private async void OnNavigateToDetail(Tracker tracker)
        {
            if (tracker != null)
            {
                IsBusy=true;
                // Make sure the tracker object and its ID are valid
                await _navigation.PushAsync(new Detail(tracker));
                IsBusy = false;
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
