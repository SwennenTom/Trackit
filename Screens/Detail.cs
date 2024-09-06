namespace Trackit.Screens;

public class Detail : ContentPage
{
	public Detail()
	{
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to the Details page!"
				}
			}
		};
	}
}