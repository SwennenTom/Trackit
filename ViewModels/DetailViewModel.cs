using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microcharts;
using SkiaSharp;
using Trackit.Models;
using Trackit.Screens;

namespace Trackit.ViewModels
{
    public class DetailViewModel : BindableObject
    {
        private int _trackerId;
        private Tracker _tracker;

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

        private readonly INavigation _navigation;
        private readonly Page _page;

        public DetailViewModel(Tracker tracker, INavigation navigation, Page page)
        {
            _trackerId = tracker.tracker_id;
            _tracker = tracker;
            DeleteTrackerCommand = new Command(async () => await DeleteTrackerAsync());
            AddValueCommand = new Command(async () => await AddValueAsync());
            NavigateToValuesCommand = new Command(async () => await NavigateToValuesAsync());
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettingsAsync());
            LoadChartDataAsync();
            _navigation = navigation;
            _page = page;
        }

        private async Task DeleteTrackerAsync()
        {
            await App.Database.DeleteTrackerAsync(_trackerId);
            await _navigation.PopAsync();
        }

        private async Task AddValueAsync()
        {
            string result = await _page.DisplayPromptAsync(
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
            await _navigation.PushAsync(new Values(_tracker));
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
