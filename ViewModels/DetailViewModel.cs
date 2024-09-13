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


        public async Task LoadChartDataAsync()
        {
            try
            {
                IsBusy = true;

                var readings = await App.Database.GetValuesForTrackerAsync(_trackerId);
                var trackerSettings = await App.Database.GetSettingsAsync(_trackerId);

                if (readings == null || trackerSettings == null)
                {
                    NoDataMessage = "Data retrieval error.";
                    return;
                }

                var plotModel = new PlotModel { Title = _tracker.name };

                if (readings.Any())
                {
                    var lineSeries = new LineSeries
                    {
                        Title = "Values",
                        Color = OxyColors.SkyBlue,
                        StrokeThickness = 2
                    };

                    foreach (var reading in readings)
                    {
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(reading.date), reading.value));
                    }

                    plotModel.Series.Add(lineSeries);

                    int emaPeriod = 7; // Define your EMA period here
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
                        emaSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(readings[i].date), emaValues[i]));
                    }

                    plotModel.Series.Add(emaSeries);

                    // Add axes
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

                    // Invalidate plot to recalculate axis ranges
                    plotModel.InvalidatePlot(true);

                    // Add LineAnnotations for thresholds
                    if (trackerSettings != null)
                    {
                        double minThreshold = trackerSettings.min_threshhold;
                        double maxThreshold = trackerSettings.max_threshold;

                        if (minThreshold != 0)
                        {
                            var minThresholdAnnotation = new LineAnnotation
                            {
                                Text = $"Min Threshold ({minThreshold})",
                                Type = LineAnnotationType.Horizontal,
                                LineStyle = LineStyle.Dash,
                                Color = OxyColors.Red,
                                StrokeThickness = 3,
                                Y = minThreshold,
                                X = 0
                            };
                            plotModel.Annotations.Add(minThresholdAnnotation);
                        }

                        if (maxThreshold != 0)
                        {
                            var maxThresholdAnnotation = new LineAnnotation
                            {
                                Text = $"Max Threshold ({maxThreshold})",
                                Type = LineAnnotationType.Horizontal,
                                LineStyle = LineStyle.Dash,
                                Color = OxyColors.Red,
                                StrokeThickness = 3,
                                Y = maxThreshold,
                                X = 0
                            };
                            plotModel.Annotations.Add(maxThresholdAnnotation);
                        }
                    }
                    else
                    {
                        NoDataMessage = "No settings found.";
                    }

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
                System.Diagnostics.Debug.WriteLine($"Error loading chart data: {ex}");

                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    "An error occurred while loading the chart data. Please try again later.",
                    "OK"
                );

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
