using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microcharts;
using SkiaSharp;
using Trackit.Models;

namespace Trackit.ViewModels
{
    //[QueryProperty(nameof(TrackerId), "trackerId")]
    public class DetailViewModel : BindableObject
    {
        private int _trackerId;

        
        //public int TrackerId
        //{
        //    get => _trackerId;
        //    set
        //    {
        //        _trackerId = value;
        //        LoadTrackerDataAsync();
        //    }
        //}

        // Other properties and fields
        private LineChart _chart;
        public LineChart Chart
        {
            get => _chart;
            set
            {
                _chart = value;
                OnPropertyChanged();
            }
        }

        private string _noDataMessage;
        public string NoDataMessage
        {
            get => _noDataMessage;
            set
            {
                _noDataMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeleteTrackerCommand { get; }
        public ICommand AddValueCommand { get; }
        public ICommand NavigateToValuesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public DetailViewModel(int trackerId)
        {
            _trackerId = trackerId;
            DeleteTrackerCommand = new Command(async () => await DeleteTrackerAsync());
            AddValueCommand = new Command(async () => await AddValueAsync());
            NavigateToValuesCommand = new Command(async () => await NavigateToValuesAsync());
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettingsAsync());
            LoadChartDataAsync();
        }

        //private async Task LoadTrackerDataAsync()
        //{
        //    // Load tracker data based on _trackerId
        //    var tracker = await App.Database.GetTrackerAsync(_trackerId);
        //    if (tracker != null)
        //    {
        //        // Logic to populate chart and other data using the tracker
        //        await LoadChartDataAsync();
        //    }
        //}

        private async Task DeleteTrackerAsync()
        {
            await App.Database.DeleteTrackerAsync(_trackerId);
            await Shell.Current.GoToAsync("//home");
        }

        private async Task AddValueAsync()
        {
            string result = await Shell.Current.CurrentPage.DisplayPromptAsync(
                "Add Value",
                "Enter the value: ",
                "Ok",
                "Cancel",
                "0",
                -1,
                keyboard: Keyboard.Numeric);

            if (!string.IsNullOrEmpty(result) && float.TryParse(result, out float value))
            {
                var trackerValue = new TrackerValues
                {
                    tracker_id = _trackerId,
                    value = value,
                    date = DateTime.Now
                };

                await App.Database.AddValueAsync(trackerValue);
                await LoadChartDataAsync();
            }
        }

        private async Task NavigateToValuesAsync()
        {
            await Shell.Current.GoToAsync($"values?trackerId={_trackerId}");
        }

        private async Task NavigateToSettingsAsync()
        {
            await Shell.Current.GoToAsync($"settings?trackerId={_trackerId}");
        }

        private async Task LoadChartDataAsync()
        {
            var entries = new List<ChartEntry>();
            var readings = await App.Database.GetValuesForTrackerAsync(_trackerId);

            if (readings.Any())
            {
                foreach (var reading in readings)
                {
                    entries.Add(new ChartEntry(reading.value)
                    {
                        Label = reading.date.ToString("MMM dd"),
                        ValueLabel = reading.value.ToString(),
                        Color = SKColor.Parse("#68B9C0")
                    });
                }

                Chart = new LineChart
                {
                    Entries = entries,
                    LineMode = LineMode.Straight,
                    LineSize = 10,
                    PointMode = PointMode.Circle,
                    PointSize = 18,
                    BackgroundColor = SKColors.White
                };

                NoDataMessage = string.Empty;
            }
            else
            {
                Chart = null;
                NoDataMessage = "No entries yet";
            }
        }
    }
}
