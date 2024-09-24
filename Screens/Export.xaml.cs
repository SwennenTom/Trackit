using Trackit.Models;
using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class Export : ContentPage
{
	private ExportViewModel _viewmodel;
	public Export(Tracker tracker)
	{
		InitializeComponent();
        this.BackgroundImageSource = "bg1.png";
        _viewmodel = new ExportViewModel(tracker.tracker_id);
		BindingContext = _viewmodel;
	}
}