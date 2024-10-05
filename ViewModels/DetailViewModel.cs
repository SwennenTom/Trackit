using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SkiaSharp;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Trackit.Models;
using Trackit.Screens;
using OxyPlot.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Trackit.ViewModels
{
    public class DetailViewModel : BindableObject
    {
        #region Init

        private readonly INavigation _navigation;
        private readonly Page _page;
        private int _trackerId;
        private Tracker _tracker;
        private bool _isFromDateInitialized = false;
        private bool _suppressNotifications;

        private DateTime _fromDate;
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate != value)
                {
                    _fromDate = value;
                    OnPropertyChanged();
                    LoadChartDataAsync();
                }
            }
        }

        private DateTime _toDate;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate != value)
                {
                    _toDate = value;

                    if (!_suppressNotifications)
                    {
                        OnPropertyChanged();
                        LoadChartDataAsync();
                    }
                }
            }
        }

        public string? Name => _tracker?.name;

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
                OnPropertyChanged(nameof(PlotModel));
            }
        }

        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
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

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand DeleteTrackerCommand { get; }
        public ICommand AddValueCommand { get; }
        public ICommand NavigateToValuesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand LastWeekCommand { get; }
        public ICommand LastMonthCommand { get; }
        public ICommand LastYearCommand { get; }
        public ICommand AllTimeCommand { get; }
        public ICommand ShowDetailInfoCommand { get; }
        public ICommand NavigateToExportCommand { get; }

        #endregion

        #region Navigation
        private async Task NavigateToValuesAsync()
        {
            var valuesPage = new Values(_tracker);
            await _navigation.PushAsync(valuesPage);
        }

        private async Task NavigateToSettingsAsync()
        {
            var settings = await App.Database.GetSettingsAsync(_trackerId);
            if (settings != null)
            {
                await _navigation.PushAsync(new SettingsTracker(settings));
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Settings not found.", "OK");
            }
        }

        private async Task NavigateToExportAsync()
        {
            await _navigation.PushAsync(new Export(_tracker));
        }
        #endregion

        #region Constructor
        public DetailViewModel(Tracker tracker, INavigation navigation, Page page)
        {
            ToDate = DateTime.Now;

            _trackerId = tracker.tracker_id;
            _tracker = tracker;
            _navigation = navigation;
            _page = page;
            _description = tracker.description;

            DeleteTrackerCommand = new Command(async () => await DeleteTrackerAsync());
            AddValueCommand = new Command(async () => await AddValueAsync());
            NavigateToValuesCommand = new Command(async () => await NavigateToValuesAsync());
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettingsAsync());
            LastWeekCommand = new Command(SetLastWeek);
            LastMonthCommand = new Command(SetLastMonth);
            LastYearCommand = new Command(SetLastYear);
            AllTimeCommand = new Command(async () => await SetAllTimeAsync());
            ShowDetailInfoCommand = new Command(OnShowDetailInfo);
            NavigateToExportCommand = new Command(async () => await NavigateToExportAsync());

            LoadChartDataAsync();
            
        }

        #endregion

        #region ShowInfo

        private async void OnShowDetailInfo()
        {
            await App.Current.MainPage.DisplayAlert("Info", $@"
Tap ➕ to add a new value. 
Tap 🔍 to navigate to the list of values. 
Tap 🗑️ to delete the tracker. 
Tap ⚙️ to navigate to the settings. 
Tap ✉️ to navigate to the export page.", 
"Ok");
        }

        #endregion

        #region CRUD
        private async Task DeleteTrackerAsync()
        {
            bool isConfirmed = await App.Current.MainPage.DisplayAlert(
                "Delete Tracker",
                $"Are you sure you wat to delete {_tracker.name}?",
                "Yes, delete this tracker.",
                "No, keep this tracker."
                );

            if (isConfirmed)
            {
                IsBusy = true;
                await App.Database.DeleteTrackerAsync(_trackerId);
                IsBusy = false;
                await _navigation.PopAsync();
            }
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

                IsBusy = true;
                await App.Database.AddValueAsync(trackerValue);
                await LoadChartDataAsync();
                PlotModel.InvalidatePlot(true);
                IsBusy = false;
            }
        }

        #endregion

        #region Setting the date
        private void SetLastWeek()
        {
            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now;
            LoadChartDataAsync();
        }

        private void SetLastMonth()
        {
            FromDate = DateTime.Now.AddDays(-31);
            ToDate = DateTime.Now;
            LoadChartDataAsync();
        }

        private void SetLastYear()
        {
            FromDate = DateTime.Now.AddDays(-365);
            ToDate = DateTime.Now;
            LoadChartDataAsync();
        }

        private async Task SetAllTimeAsync()
        {
            var earliestDate = await GetEarliestDateFromDatabaseAsync();
            FromDate = earliestDate ?? DateTime.Now;
            ToDate = DateTime.Now;
            LoadChartDataAsync();
        }
        private async Task<DateTime?> GetEarliestDateFromDatabaseAsync()
        {
            
            var allValues = await App.Database.GetValuesForTrackerAsync(_trackerId);
            return allValues.Min(v => v.date);
        }

        #endregion

        #region Draw the graph

        private async Task<(IEnumerable<TrackerValues> readings, TrackerSettings trackerSettings)> FetchDataAsync(int trackerId)
        {
            var readings = new List<TrackerValues>();
            readings = await App.Database.GetValuesForTrackerAsync(trackerId);
            var trackerSettings = await App.Database.GetSettingsAsync(trackerId);
            return (readings, trackerSettings);
        }

        private IEnumerable<TrackerValues> FilterReadingsByDate(IEnumerable<TrackerValues> readings)
        {
            return readings.Where(r => r.date >= FromDate && r.date <= ToDate);
        }

        private PlotModel CreatePlotModel(TrackerSettings trackerSettings)
        {
            var plotModel = new PlotModel();
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM dd",
                Title = "Date",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            plotModel.Axes.Add(dateAxis);

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Value",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            plotModel.Axes.Add(valueAxis);

            AddThresholdAnnotations(plotModel, trackerSettings);

            return plotModel;
        }

        private void AddThresholdAnnotations(PlotModel plotModel, TrackerSettings trackerSettings)
        {
            if (trackerSettings.showMinThreshold)
            {
                var minThresholdAnnotation = new LineAnnotation
                {
                    Text = $"Min Threshold ({trackerSettings.min_threshhold})",
                    Type = LineAnnotationType.Horizontal,
                    LineStyle = LineStyle.Solid,
                    Color = OxyColors.Red,
                    StrokeThickness = 1,
                    Y = trackerSettings.min_threshhold
                };
                plotModel.Annotations.Add(minThresholdAnnotation);
            }

            if (trackerSettings.showMaxThreshold)
            {
                var maxThresholdAnnotation = new LineAnnotation
                {
                    Text = $"Max Threshold ({trackerSettings.max_threshold})",
                    Type = LineAnnotationType.Horizontal,
                    LineStyle = LineStyle.Solid,
                    Color = OxyColors.Red,
                    StrokeThickness = 1,
                    Y = trackerSettings.max_threshold
                };
                plotModel.Annotations.Add(maxThresholdAnnotation);
            }
        }

        private void AddSeries(PlotModel plotModel, IEnumerable<TrackerValues> readings, TrackerSettings trackerSettings)
        {
            LineSeries lineSeries = CreateLineSeries(trackerSettings);

            foreach (var reading in readings)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(reading.date), reading.value));
            }

            plotModel.Series.Add(lineSeries);

            if (trackerSettings.scatter)
            {
                var scatterSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 4,
                    MarkerFill = OxyColors.Blue
                };
                foreach (var reading in readings)
                {
                    scatterSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(reading.date), reading.value));
                }
                plotModel.Series.Add(scatterSeries);
            }
        }

        private LineSeries CreateLineSeries(TrackerSettings trackerSettings)
        {
            if (trackerSettings.stepped)
            {
                return new StairStepSeries { Color = OxyColors.SkyBlue, StrokeThickness = 2 };
            }
            else if (trackerSettings.splines)
            {
                return new LineSeries
                {
                    InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                    StrokeThickness = 2,
                    Color = OxyColors.SkyBlue
                };
            }

            return new LineSeries { Title = "Values", Color = OxyColors.SkyBlue, StrokeThickness = 2 };
        }

        private double[] CalculateEMA(double[] values, int period)
        {
            var ema = new double[values.Length];
            if (values.Length == 0 || period <= 0)
                return ema;

            double multiplier = 2.0 / (period + 1);
            ema[0] = values[0]; // Start with the first value

            for (int i = 1; i < values.Length; i++)
            {
                ema[i] = (values[i] - ema[i - 1]) * multiplier + ema[i - 1];
            }

            return ema;
        }

        private void AddEMASeries(PlotModel plotModel, IEnumerable<TrackerValues> readings, TrackerSettings trackerSettings)
        {
            if (trackerSettings.showTrendLine)
            {
                int emaPeriod = 7;
                var emaValues = CalculateEMA(readings.Select(r => (double)r.value).ToArray(), emaPeriod);
                var emaSeries = new LineSeries
                {
                    Title = $"EMA ({emaPeriod})",
                    Color = OxyColors.Orange,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Dash
                };

                for (int i = emaPeriod - 1; i < emaValues.Length; i++)
                {
                    emaSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(readings.ElementAt(i).date), emaValues[i]));
                }

                plotModel.Series.Add(emaSeries);
            }
        }

        public async Task LoadChartDataAsync()
        {
            try
            {
                IsBusy = true;

                var (readings, trackerSettings) = await FetchDataAsync(_trackerId);

                var filteredReadings = FilterReadingsByDate(readings);

                _suppressNotifications = true;
                ToDate = DateTime.Now;
                _suppressNotifications = false;

                if (readings == null || trackerSettings == null)
                {
                    NoDataMessage = "Data retrieval error.";
                    return;
                }

                if (!readings.Any())
                {
                    NoDataMessage = "No values yet";
                    return;
                }

                if(!filteredReadings.Any())
                {
                    NoDataMessage = "No values in the selected date range";
                    return;
                }

                if (!_isFromDateInitialized)
                {
                    FromDate = readings.Min(r => r.date);
                    ToDate = readings.Max(r => r.date);
                    _isFromDateInitialized = true;
                }


                var plotModel = CreatePlotModel(trackerSettings);

                AddSeries(plotModel, filteredReadings, trackerSettings);
                AddEMASeries(plotModel, filteredReadings, trackerSettings);

                    PlotModel = plotModel;
                    PlotModel.InvalidatePlot(true);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading chart data: {ex}");
                await App.Current.MainPage.DisplayAlert("Error", "An error occurred while loading the chart data.", "OK");
                NoDataMessage = "An error occurred while loading data.";
            }
            finally
            {
                IsBusy = false;
            }
        }


        #endregion

    }
}
