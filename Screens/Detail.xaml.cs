using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using OxyPlot.Maui.Skia;
using Trackit.Models;
using Trackit.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using OxyPlot.Axes;

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
