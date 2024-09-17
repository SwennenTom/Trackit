using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class CreateTracker : ContentPage
{
	public CreateTracker()
	{
		InitializeComponent();
        this.BackgroundImageSource = "bg1.png";
        BindingContext = new CreateTrackerViewModel();
	}
}