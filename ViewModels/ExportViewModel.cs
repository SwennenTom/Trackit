using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using Syncfusion.XlsIO;
using Trackit.Models;
using OxyPlot.SkiaSharp;
using OxyPlot;
using Syncfusion.XlsIO.Parser.Biff_Records;


namespace Trackit.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        #region Init
        private int _trackerId;
        private string _emailrecipient;

        public string EmailRecipient
        {
            get => _emailrecipient;
            set
            {
                _emailrecipient = value;
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

        #region constructor
        public ExportViewModel(int trackerId)
        {
            _trackerId = trackerId;

            LastWeekCommand = new Command(SetLastWeek);
            LastMonthCommand = new Command(SetLastMonth);
            LastYearCommand = new Command(SetLastYear);
            AllTimeCommand = new Command(async () => await SetAllTimeAsync());
            ExportCommand = new Command(OnExport);

        }

        #endregion

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

        #region Create excel file and path

        #region Create filepath for excelfile
        private string GetFilePath(Tracker tracker)
        {
            string fileName = tracker.name + ".xlsx";
            string folderName = "Trackit";
            string libraryPath = string.Empty;

            // Get platform-specific storage path using DeviceInfo.Platform in MAUI
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // On Android, create a Trackit folder inside the Personal folder
                libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), folderName);
                if (!Directory.Exists(libraryPath))
                {
                    Directory.CreateDirectory(libraryPath); // Create the Trackit folder if it doesn't exist
                }
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // For iOS, create a Trackit folder inside MyDocuments folder
                libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);
                if (!Directory.Exists(libraryPath))
                {
                    Directory.CreateDirectory(libraryPath); // Create the Trackit folder if it doesn't exist
                }
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                // For Windows/WinUI, create a Trackit folder inside LocalApplicationData
                libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
                if (!Directory.Exists(libraryPath))
                {
                    Directory.CreateDirectory(libraryPath); // Create the Trackit folder if it doesn't exist
                }
            }

            // Combine the folder path with the file name
            return Path.Combine(libraryPath, fileName);
        }
        #endregion

        #region Create Excelfile
        private async Task<MemoryStream> CreateExcelPage(Tracker tracker, List<TrackerValues> valuesInRange)
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
                    worksheet2.Range["A" + rowIndex].DateTime = value.date; // Set DateTime
                    worksheet2.Range["B" + rowIndex].Number = value.value; // Add Tracker Value
                    rowIndex++;
                }


                // Auto-fit columns
                worksheet2.AutofitColumn(1);
                worksheet2.AutofitColumn(2);

                IChartShape chart = worksheet1.Charts.Add(); // Adjust the position and size as needed
                chart.ChartType = ExcelChartType.Line; // Set type of chart
                int lastRow = rowIndex -1;
                chart.DataRange = worksheet2.Range["A2:B" + lastRow]; // Set chart data

                // Set chart title and axes titles
                chart.PrimaryValueAxis.Title = "Values";
                chart.PrimaryCategoryAxis.Title = "Date";
                chart.PrimaryCategoryAxis.NumberFormat = "yyyy-MM-dd"; // Format the axis as dates
                chart.PrimaryCategoryAxis.CategoryLabels = worksheet2.Range["A2:A" + lastRow]; // Explicitly set category labels


                //Set Datalabels
                IChartSerie serie1 = chart.Series[0];
                serie1.DataPoints.DefaultDataPoint.DataLabels.IsValue = true;
                serie1.DataPoints.DefaultDataPoint.DataLabels.Position = ExcelDataLabelPosition.Left;

                // Set the chart legend
                chart.HasLegend = true;
                chart.Legend.Position = ExcelLegendPosition.Bottom;

                MemoryStream excelStream = new MemoryStream();
                workbook.SaveAs(excelStream);
                excelStream.Position = 0;

                return excelStream;
            }
        }
        #endregion

        #endregion

        #region Send Email 
        private async Task SendEmailWithAttachment(Tracker tracker, List<TrackerValues> valuesInRange)
        {
            if (Email.Default.IsComposeSupported)
            {
                MemoryStream excelStream = await CreateExcelPage(tracker, valuesInRange);
                byte[] excelFileBytes = excelStream.ToArray();
                string excelFileName = tracker.name + ".xlsx";

                string tempFilePath = Path.Combine(FileSystem.CacheDirectory, excelFileName);
                tempFilePath = GetFilePath(tracker);
                File.WriteAllBytes(tempFilePath, excelFileBytes);

                var message = new EmailMessage
                {
                    Subject = tracker.name + ": " + tracker.description,
                    Body = Message,
                    To = new List<string> { EmailRecipient }
                };

                message.Attachments.Add(new EmailAttachment(tempFilePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

                await Email.ComposeAsync(message);

                File.Delete(tempFilePath);
        }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
        "Email Not Supported",
        "Something went wrong when we tried creating the email.",
        "OK");
    }
}

        #endregion

        #region Export logic
        private async void OnExport()
        {
            Tracker tracker = await App.Database.GetTrackerAsync(_trackerId);
            List<TrackerValues> valuesTotal = await App.Database.GetValuesForTrackerAsync(_trackerId);
            List<TrackerValues> valuesInRange = valuesTotal
                                                    .Where(v => v.date >= FromDate && v.date <= ToDate)
                                                    .ToList();

            await SendEmailWithAttachment(tracker, valuesInRange);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
