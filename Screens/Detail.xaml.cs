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
    }
}
