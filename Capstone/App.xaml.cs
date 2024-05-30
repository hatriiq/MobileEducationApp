using Microsoft.Maui.Controls;
using SQLite;
using System.IO;
using Microsoft.Maui.Storage;
using static C971.Database;
using Plugin.LocalNotification;

namespace C971
{
    public partial class App : Application   //App class inherits from Application Example of Inheritance.
    {
        public static string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "MyApp.db");

        public App()
        {
            InitializeComponent();
            InitializeDatabase();
            MainPage = new NavigationPage(new LoginPage());
            LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        private static SQLiteConnection GetDatabaseConnection()
        {
            return new SQLiteConnection(DatabasePath);
        }

        private static void InitializeDatabase()
        {
            using (var db = GetDatabaseConnection())
            {
                db.RunInTransaction(() =>
                {
                    db.CreateTable<Term>();
                    db.CreateTable<Course>();
                    db.CreateTable<Assessment>();
                });
            }
        }
    }
}



