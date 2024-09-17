using Trackit.Models;
using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class SettingsTracker : ContentPage
{
	public SettingsTracker(TrackerSettings settings)
	{
		InitializeComponent();
        this.BackgroundImageSource = "bg1.png";
        BindingContext = new SettingsTrackerViewModel(settings);
	}
}