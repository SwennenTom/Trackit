using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Trackit;
using Trackit.Models;
using Trackit.Screens;

public class EditValuesViewModel : INotifyPropertyChanged
{
    private readonly INavigation _navigation;
    private TrackerValues _trackerValue;

    public float Value { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }

    public Command SaveCommand { get; }
    public Command CancelCommand { get; }

    public EditValuesViewModel(INavigation navigation, TrackerValues trackerValue)
    {
        _navigation = navigation;
        _trackerValue = trackerValue;

        Value = trackerValue.value;
        Date = trackerValue.date.Date;
        Time = trackerValue.date.TimeOfDay;

        SaveCommand = new Command(OnSave);
        CancelCommand = new Command(OnCancel);
    }

    private async void OnSave()
    {
        _trackerValue.value = Value;
        _trackerValue.date = Date.Add(Time);

        await App.Database.UpdateValueAsync(_trackerValue);

        await _navigation.PopAsync();
    }

    private async void OnCancel()
    {
        // Cancel editing, just navigate back
        await _navigation.PopAsync();
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
