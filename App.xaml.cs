using Trackit.Data;
using Trackit.Properties;
using Trackit.Screens;

namespace Trackit
{
    public partial class App : Application
    {

        static Database database;
        public App()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXZcdXRRQmBfVU11WEM=");
            MainPage = new NavigationPage(new Home());
        }

        public static Database Database
        {
            get
            {
                if(database == null)
                {
                    database = new Database(Constants.DatabasePath);
                }
                return database;
            }
        }
    }
}
