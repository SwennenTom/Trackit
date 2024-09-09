using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcharts;
using SkiaSharp;
using Trackit.Models;
using Trackit.Screens;

namespace Trackit.ViewModels
{
    public class DetailViewModel : BindableObject
    {
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

        private readonly Tracker _tracker;
        public string Name => _tracker.name;
        public DetailViewModel(Tracker tracker)
        {
            _tracker = tracker;
            LoadChartDataAsync();
        }

        public DetailViewModel()
        {
            LoadChartDataAsync();
        }


        private async Task LoadChartDataAsync()
        {
            var entries = new List<ChartEntry>();

            var readings = await App.Database.GetValuesForTrackerAsync(_tracker.tracker_id);

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
                    LineSize = 8,
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
