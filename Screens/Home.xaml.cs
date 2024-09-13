using Trackit.ViewModels;
using Microsoft.Maui.Controls;

namespace Trackit.Screens;

public partial class Home : ContentPage
{
    public Home()
    {
        InitializeComponent();

        BindingContext = new HomeViewModel(this.Navigation);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var viewModel = BindingContext as HomeViewModel;

        if (viewModel != null)
        {
            viewModel.LoadTrackers();
        }
    }

}