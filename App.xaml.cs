using Trackit.Data;
using Trackit.Properties;

namespace Trackit
{
    public partial class App : Application
    {

        static Database database;
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
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
