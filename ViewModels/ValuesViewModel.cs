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

        public ObservableCollection<TrackerValues> trackerValues { get; set; }

        public ICommand EditValueCommand { get; set; }
        public ICommand DeleteValueCommand { get; set; }


        public ValuesViewModel(Tracker tracker)
        {
            _tracker = tracker;
            LoadTrackerValues();

            EditValueCommand = new Command<TrackerValues>(async (value) => await OnEditValue(value));
            DeleteValueCommand = new Command<TrackerValues>(async (value) => await OnDeleteValue(value));
        }

        public async Task LoadTrackerValues()
        {
            trackerValues.Clear();
            var valuesFromDb = await App.Database.GetValuesForTrackerAsync(_tracker.tracker_id);

            foreach (var value in valuesFromDb)
            {
                trackerValues.Add(value);
            }
        }
        private async Task OnEditValue(TrackerValues value)
        {
            // Logic to edit the value, e.g., showing a prompt and updating the database
        }

        private async Task OnDeleteValue(TrackerValues value)
        {
            await App.Database.DeleteValueAsync(value);
            //TrackerValues.Remove(value); // Update the collection after deletion
        }
    }
}
