using Microsoft.Maui.Controls;
using SQLite;
using System.IO;
using Microsoft.Maui.Storage;
using static C971.Database;
using Plugin.LocalNotification;

namespace C971
{
    public partial class App : Application
    {
        public static string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "MyApp.db");
        private static readonly SQLiteConnection db = new SQLiteConnection(DatabasePath);

        public App()
        {
            InitializeComponent();
            InitializeDatabase();
            MainPage = new NavigationPage(new MainPage());
            LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        private static void InitializeDatabase()
        {
            db.CreateTable<Term>();
            db.CreateTable<Course>();
        }
    }
}


