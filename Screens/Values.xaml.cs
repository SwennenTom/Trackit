using Trackit.Models;
using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class Values : ContentPage
{
	private Tracker _tracker;
	public Values(Tracker tracker)
	{
		InitializeComponent();
		_tracker = tracker;
        BindingContext = new ValuesViewModel(this.Navigation, tracker);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var viewModel = BindingContext as ValuesViewModel;

        if (viewModel != null)
        {
            viewModel.LoadTrackerValues();
        }
    }
}