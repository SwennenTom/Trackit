﻿using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.XlsIO;
using Trackit.Models;
using OxyPlot.SkiaSharp;
using OxyPlot;


namespace Trackit.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        #region Init
        private int _trackerId;
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private DateTime _fromDate = DateTime.Now.AddDays(-1);
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _toDate = DateTime.Now;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Commands
        public ICommand ExportCommand { get; }
        public ICommand LastWeekCommand { get; }
        public ICommand LastMonthCommand { get; }
        public ICommand LastYearCommand { get; }
        public ICommand AllTimeCommand { get; }

        #endregion

        public ExportViewModel(int trackerId)
        {
            _trackerId = trackerId;

            LastWeekCommand = new Command(SetLastWeek);
            LastMonthCommand = new Command(SetLastMonth);
            LastYearCommand = new Command(SetLastYear);
            AllTimeCommand = new Command(async () => await SetAllTimeAsync());
            ExportCommand = new Command(OnExport);

        }

        #region Set Dates
        private void SetLastWeek()
        {
            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now;
        }

        private void SetLastMonth()
        {
            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;
        }

        private void SetLastYear()
        {
            FromDate = DateTime.Now.AddYears(-1);
            ToDate = DateTime.Now;
        }

        private async Task SetAllTimeAsync()
        {
            var earliestDate = await GetEarliestDateFromDatabaseAsync();
            FromDate = earliestDate ?? DateTime.Now;
            ToDate = DateTime.Now;
            ToDate = DateTime.Now;
        }

        private async Task<DateTime?> GetEarliestDateFromDatabaseAsync()
        {

            var allValues = await App.Database.GetValuesForTrackerAsync(_trackerId);
            return allValues.Min(v => v.date);
        }

        #endregion

        private async Task<MemoryStream> CreateExcelPage(Tracker tracker, List<TrackerValues> valuesInRange, PlotModel plotModel)
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                Syncfusion.XlsIO.IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;

                // Create a workbook with two worksheets
                IWorkbook workbook = application.Workbooks.Create(2);
                IWorksheet worksheet1 = workbook.Worksheets[0];
                IWorksheet worksheet2 = workbook.Worksheets[1];

                // Name the sheets
                worksheet1.Name = tracker.name;
                worksheet2.Name = "Data";

                // Title in the top left
                worksheet1.Range["A1"].Text = tracker.name;
                worksheet1.Range["A1"].CellStyle.Font.Bold = true;
                worksheet1.Range["A1"].CellStyle.Font.Size = 16;

                // Description below the title
                worksheet1.Range["A2"].Text = tracker.description;
                worksheet1.Range["A2"].CellStyle.Font.Italic = true;
                worksheet1.Range["A2"].CellStyle.Font.Size = 12;

                // Fill the second sheet with data values and dates
                worksheet2.Range["A1"].Text = "Date";
                worksheet2.Range["B1"].Text = "Value";
                worksheet2.Range["A1:B1"].CellStyle.Font.Bold = true;

                int rowIndex = 2;
                foreach (var value in valuesInRange)
                {
                    worksheet2.Range["A" + rowIndex].Text = value.date.ToString("yyyy-MM-dd"); // Add Date
                    worksheet2.Range["B" + rowIndex].Number = value.value; // Add Tracker Value
                    rowIndex++;
                }

                // Auto-fit columns
                worksheet2.AutofitColumn(1);
                worksheet2.AutofitColumn(2);

                IChart chart = worksheet1.Charts.Add(ExcelChartType.Line, 3, 1, 10, 5); // Adjust the position and size as needed

                // Set chart data
                chart.Series.Add(worksheet2.Range["A1:B" + (rowIndex - 1)], ExcelChartSeriesType.Line);

                // Set chart title and axes titles
                chart.Title.Text = "Glucose Levels Over Time";
                chart.PrimaryValueAxis.Title.Text = "Glucose Level";
                chart.PrimaryCategoryAxis.Title.Text = "Date";

                // Set the chart legend
                chart.HasLegend = true;
                chart.Legend.Position = ExcelLegendPosition.Bottom;

                MemoryStream excelStream = new MemoryStream();
                workbook.SaveAs(excelStream);
                excelStream.Position = 0;  // Reset position to the beginning of the stream

                return excelStream;
            }
        }

        private async Task SendEmailWithAttachment(Tracker tracker, List<TrackerValues> valuesInRange, PlotModel plotModel)
        {
            MemoryStream excelStream = await CreateExcelPage(tracker, valuesInRange, plotModel);
            byte[] excelFileBytes = excelStream.ToArray();
            string excelFileName = tracker.name + ".xlsx";

            await Email.ComposeAsync(
        subject: "Tracker Data for " + tracker.name,
        body: "Please find attached the tracker data and graph.",
        to: new string[] { Email },
        attachments: new List<EmailAttachment> { new EmailAttachment(excelFileName, excelFileBytes) }
    );
        }

        private async void OnExport()
        {
            Tracker tracker = await App.Database.GetTrackerAsync(_trackerId);
            List<TrackerValues> valuesTotal = await App.Database.GetValuesForTrackerAsync(_trackerId);
            List<TrackerValues> valuesInRange = valuesTotal
                                                    .Where(v => v.date >= FromDate && v.date <= ToDate)
                                                    .ToList();
            PlotModel plotModel = CreatePlotModel(valuesInRange);

            await SendEmailWithAttachment(tracker, valuesInRange, plotModel);
            // Logic for generating the Excel report and opening email client
            // For now, display a simple message as a placeholder
            await App.Current.MainPage.DisplayAlert("Export", "The report will be generated and sent via email.", "OK");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
