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
        BindingContext = new ValuesViewModel(tracker);
    }
}