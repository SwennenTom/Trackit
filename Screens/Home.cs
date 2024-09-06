namespace Trackit.Screens;

public class Home : ContentPage
{
	public Home()
	{
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to the Home page!"
				}
			}
		};
	}
}