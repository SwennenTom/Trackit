using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Trackit.Models;

namespace Trackit.ViewModels
{
    class ValuesViewModel : BindableObject
    {
        private Tracker _tracker;

        public ObservableCollection<TrackerValues> trackerValues { get; set; } = new ObservableCollection<TrackerValues>();

        public ICommand EditValueCommand { get; set; }
        public ICommand DeleteValueCommand { get; set; }

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


        public ValuesViewModel(Tracker tracker)
        {
            _tracker = tracker;

            EditValueCommand = new Command<TrackerValues>(async (value) => await OnEditValue(value));
            DeleteValueCommand = new Command<TrackerValues>(async (value) => await OnDeleteValue(value));

            //LoadTrackerValues();
        }

        public async Task LoadTrackerValues()
        {
            IsBusy = true;
            trackerValues.Clear();
            var valuesFromDb = await App.Database.GetValuesForTrackerAsync(_tracker.tracker_id);

            foreach (var value in valuesFromDb)
            {
                trackerValues.Add(value);
            }
            IsBusy = false;
        }
        private async Task OnEditValue(TrackerValues value)
        {
            // Logic to edit the value, e.g., showing a prompt and updating the database
        }

        private async Task OnDeleteValue(TrackerValues value)
        {
            bool isConfirmed = await App.Current.MainPage.DisplayAlert(
                "Delete value",
                "Are you sure you want to delete this value?",
                "Yes, delete the value.",
                "No, keep the value."
                );

            if (isConfirmed)
            {
                IsBusy=true;
                await App.Database.DeleteValueAsync(value);
                trackerValues.Clear();
                await LoadTrackerValues();
                IsBusy=false;
            }
        }
    }
}
