using Trackit.Models;
using Trackit.ViewModels;

namespace Trackit.Screens
{
    public partial class Detail : ContentPage
    {
        public Detail(Tracker tracker)
        {
            InitializeComponent();
            BindingContext = new DetailViewModel(tracker, this.Navigation, this);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var viewModel = BindingContext as DetailViewModel;

            if (viewModel != null)
            {
                viewModel.LoadChartDataAsync();
            }
        }
    }
}
