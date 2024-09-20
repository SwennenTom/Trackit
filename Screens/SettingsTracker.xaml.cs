using Trackit.Models;
using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class SettingsTracker : ContentPage
{
    private SettingsTrackerViewModel _viewModel;
    public SettingsTracker(TrackerSettings settings)
	{
        InitializeComponent();
        _viewModel = new SettingsTrackerViewModel(settings);
        BindingContext = _viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_viewModel.SaveCommand.CanExecute(null))
        {
            _viewModel.SaveCommand.Execute(null);
        }
    }
}