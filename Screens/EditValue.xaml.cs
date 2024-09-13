using Trackit.Models;
using Trackit.ViewModels;

namespace Trackit.Screens;

public partial class EditValue : ContentPage
{
	public EditValue(TrackerValues trackerValue)
	{
		InitializeComponent();
		BindingContext = new EditValuesViewModel(this.Navigation, trackerValue);
	}
}