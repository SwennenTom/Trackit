namespace Trackit.Screens;

public class Values : ContentPage
{
	public Values()
	{
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to the Values page"
				}
			}
		};
	}
}