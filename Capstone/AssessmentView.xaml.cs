using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using SQLite;
using System;
using System.Linq;
using static C971.Database;
using static C971.MainPage;

namespace C971
{
    public partial class AssessmentView : ContentPage
    {
        private readonly Action _updateCourseCallback;
        private readonly SQLiteConnection _db;
        private Assessment _currentAssessment;

        public AssessmentView(int assessmentId, int courseId, Action updateCourseCallback)
        {
            InitializeComponent();

            _db = new SQLiteConnection(MainPage.DatabasePath);
            _currentAssessment = _db.Find<Assessment>(assessmentId) ?? new Assessment
            {
                CourseId = courseId,
                Name = "New Assessment",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Type = "Performance"
            };

            _updateCourseCallback = updateCourseCallback;
            LoadAssessmentDetails();
            LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        private void LoadAssessmentDetails()
        {
            assessmentName.Text = _currentAssessment.Name ?? "New Assessment";
            assessmentStart.Date = _currentAssessment.StartDate.Year >= DateTime.Now.Year ? _currentAssessment.StartDate : DateTime.Now;
            assessmentEnd.Date = _currentAssessment.EndDate.Year >= DateTime.Now.Year ? _currentAssessment.EndDate : DateTime.Now.AddDays(30);
            assessmentType.SelectedItem = _currentAssessment.Type ?? "Performance";
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(assessmentName.Text) || assessmentType.SelectedItem == null)
            {
                DisplayAlert("Invalid Input", "Please ensure all fields are filled correctly.", "OK");
                return;
            }

            SaveAssessment();
            _updateCourseCallback?.Invoke();
            DisplayAlert("Success", "Assessment details updated successfully.", "OK");
        }

        private void SaveAssessment()
        {
            _currentAssessment.Name = assessmentName.Text;
            _currentAssessment.StartDate = assessmentStart.Date;
            _currentAssessment.EndDate = assessmentEnd.Date;
            _currentAssessment.Type = assessmentType.SelectedItem.ToString();
            _db.InsertOrReplace(_currentAssessment);
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            _db.Delete(_currentAssessment);
            _updateCourseCallback?.Invoke();
            DisplayAlert("Success", "Assessment deleted successfully.", "OK");
            Navigation.PopAsync();
        }

        private void OnStartAlertClicked(object sender, EventArgs e)
        {
            var notifyTime = _currentAssessment.StartDate.Date.Add(new TimeSpan(8, 0, 0));
            if (notifyTime >= DateTime.Now)
            {
                var notification = new NotificationRequest
                {
                    NotificationId = _currentAssessment.AssessmentId,
                    Title = "Assessment Start Alert",
                    Description = $"The assessment '{_currentAssessment.Name}' is starting today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    }
                };
                LocalNotificationCenter.Current.Show(notification);
                DisplayAlert("Success", "Start alert set successfully.", "OK");
            }
            else
            {
                DisplayAlert("Error", $"The start date {notifyTime} is in the past. Cannot set an alert.", "OK");
            }
        }
      
    
        private void OnEndAlertClicked(object sender, EventArgs e)
        {
            var notifyTime = _currentAssessment.EndDate.Date.Add(new TimeSpan(17, 0, 0));
            if (notifyTime >= DateTime.Now)
            {
                var notification = new NotificationRequest
                {
                    NotificationId = _currentAssessment.AssessmentId + 1000,
                    Title = "Assessment End Alert",
                    Description = $"The assessment '{_currentAssessment.Name}' is ending today!",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    }
                };
                LocalNotificationCenter.Current.Show(notification);
                DisplayAlert("Success", "End alert set successfully.", "OK");
            }
            else
            {
                DisplayAlert("Error", $"The end date {notifyTime} is in the past. Cannot set an alert.", "OK");
            }
        }
    }
}

