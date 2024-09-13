using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class CreateTracker : ContentPage
{
	public CreateTracker()
	{
		InitializeComponent();
		BindingContext = new CreateTrackerViewModel();
	}
}