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
