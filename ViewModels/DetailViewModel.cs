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

namespace Trackit.ViewModels
{
    public class DetailViewModel : BindableObject
    {
        private int _trackerId;
        private Tracker _tracker;
        public string? Name => _tracker?.name;

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
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
            _navigation = navigation;
            _page = page;

            DeleteTrackerCommand = new Command(async () => await DeleteTrackerAsync());
            AddValueCommand = new Command(async () => await AddValueAsync());
            NavigateToValuesCommand = new Command(async () => await NavigateToValuesAsync());
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettingsAsync());

            LoadChartDataAsync();
            
        }

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
                IsBusy = false;
            }
        }

        private async Task NavigateToValuesAsync()
        {
            var valuesPage = new Values(_tracker);
            await _navigation.PushAsync(valuesPage);
        }

        private async Task NavigateToSettingsAsync()
        {
            await Shell.Current.GoToAsync($"settings?trackerId={_trackerId}");
        }

        public async Task LoadChartDataAsync()
        {
            try
            {
                IsBusy = true;

                var readings = await App.Database.GetValuesForTrackerAsync(_trackerId);

                var trackerSettings = await App.Database.GetSettingsAsync(_trackerId);
                var plotModel = new PlotModel { Title = _tracker.name };

                if (readings.Any())
                {
                    var lineSeries = new LineSeries
                    {
                        Title = "Values",
                        Color = OxyColors.SkyBlue,
                        StrokeThickness = 2
                    };

                    var values = readings.Select(r => (double)r.value).ToArray();
                    var dates = readings.Select(r => DateTimeAxis.ToDouble(r.date)).ToArray();

                    foreach (var reading in readings)
                    {
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(reading.date), reading.value));
                    }

                    plotModel.Series.Add(lineSeries);

                    int emaPeriod = 7; // Define your EMA period here
                    var emaValues = CalculateEMA(values, emaPeriod);
                    var emaSeries = new LineSeries
                    {
                        Title = $"EMA ({emaPeriod})",
                        Color = OxyColors.Orange,
                        StrokeThickness = 2,
                        LineStyle = LineStyle.Dash
                    };

                    for (int i = emaPeriod - 1; i < emaValues.Length; i++)
                    {
                        emaSeries.Points.Add(new DataPoint(dates[i], emaValues[i]));
                    }

                    plotModel.Series.Add(emaSeries);

                    
                    if (trackerSettings != null)
                    {
                        double minThreshold = trackerSettings.min_threshhold;
                        double maxThreshold = trackerSettings.max_threshold;

                        if (minThreshold != 0) // Use appropriate logic to check if the threshold should be displayed
                        {
                            var minThresholdSeries = new LineSeries
                            {
                                Title = $"Min Threshold ({minThreshold})",
                                Color = OxyColors.Red,
                                StrokeThickness = 3,
                                LineStyle = LineStyle.Dash
                            };
                            minThresholdSeries.Points.Add(new DataPoint(plotModel.Axes[0].ActualMinimum, minThreshold));
                            minThresholdSeries.Points.Add(new DataPoint(plotModel.Axes[0].ActualMaximum, minThreshold));
                            plotModel.Series.Add(minThresholdSeries);
                        }

                        if (maxThreshold != 0) // Use appropriate logic to check if the threshold should be displayed
                        {
                            var maxThresholdSeries = new LineSeries
                            {
                                Title = $"Max Threshold ({maxThreshold})",
                                Color = OxyColors.Red,
                                StrokeThickness = 3,
                                LineStyle = LineStyle.Dash
                            };
                            maxThresholdSeries.Points.Add(new DataPoint(plotModel.Axes[0].ActualMinimum, maxThreshold));
                            maxThresholdSeries.Points.Add(new DataPoint(plotModel.Axes[0].ActualMaximum, maxThreshold));
                            plotModel.Series.Add(maxThresholdSeries);
                        }
                    }
                    else
                    {
                        NoDataMessage = "No settings found.";
                    }

                    plotModel.Axes.Add(new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        StringFormat = "MMM dd",
                        Title = "Date",
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot
                    });
                    plotModel.Axes.Add(new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "Value",
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot
                    });

                }
                else
                {
                    NoDataMessage = "No values yet";
                }

                
                PlotModel = plotModel;
                PlotModel.InvalidatePlot(true); // Force a redraw
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading chart data: {ex.Message}");

                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    "An error occurred while loading the chart data. Please try again later.",
                    "OK"
                );

                // Ensure the UI state is correctly set
                NoDataMessage = "An error occurred while loading data.";
            }
            finally
            {
                IsBusy = false;
            }
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


    }
}
