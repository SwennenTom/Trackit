namespace Trackit.Screens;

public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();

		BindingContext = new Trackit.ViewModels.HomeViewModel();
	}
}