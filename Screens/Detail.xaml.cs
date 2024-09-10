using Trackit.ViewModels;

namespace Trackit.Screens
{
    [QueryProperty(nameof(TrackerId), "trackerId")]
    public partial class Detail : ContentPage
    {
        public int TrackerId { get; set; }

        public Detail()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (TrackerId > 0)
            {
                BindingContext = new DetailViewModel(TrackerId);
            }
            else
            {
                // Handle the case when the trackerId is missing
                DisplayAlert("Error", "Tracker ID not found!", "OK");
            }
        }
    }
}
