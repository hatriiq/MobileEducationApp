using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using SQLite;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using static C971.Database;

namespace C971
{
    public partial class CourseView : ContentPage
    {
        private List<Assessment> _assessments;
        private Course _currentCourse;
        private readonly SQLiteConnection _db;

        public CourseView(int courseId)
        {
            InitializeComponent();
            _db = new SQLiteConnection(MainPage.DatabasePath);
            _currentCourse = _db.Find<Course>(courseId) ?? MainPage.CourseList[courseId];
            LoadAssessments();
            LoadCourseDetails();
            LocalNotificationCenter.Current.RequestNotificationPermission();
            courseStat.SelectedIndexChanged += OnStatusChanged;
        }

        private void LoadAssessments()
        {
            _assessments = _db.Table<Assessment>().Where(a => a.CourseId == _currentCourse.CourseId).ToList();
            RefreshAssessmentButtons();
        }

        private void RefreshAssessmentButtons()
        {
            assessmentsLayout.Children.Clear();
            bool canAddPerformance = true;
            bool canAddObjective = true;

            foreach (var assessment in _assessments)
            {
                var assessmentButton = new Button
                {
                    Text = assessment.Name,
                    BackgroundColor = Colors.Black,
                    TextColor = Colors.White,
                    CommandParameter = assessment.AssessmentId
                };
                assessmentButton.Clicked += OnAssessmentButtonClicked;
                assessmentsLayout.Children.Add(assessmentButton);

                if (assessment.Type == "Performance")
                    canAddPerformance = false;
                if (assessment.Type == "Objective")
                    canAddObjective = false;
            }

            if (canAddPerformance || canAddObjective)
            {
                var addAssessmentButton = new Button
                {
                    Text = "Add Assessment",
                    BackgroundColor = Colors.Black,
                    TextColor = Colors.White
                };
                addAssessmentButton.Clicked += (sender, args) => OnAddAssessmentClicked(canAddPerformance, canAddObjective);
                assessmentsLayout.Children.Add(addAssessmentButton);
            }
        }


        private void LoadCourseDetails()
        {
            courseName.Text = _currentCourse.CourseName;
            courseStart.Date = _currentCourse.Start;
            courseEnd.Date = _currentCourse.End;
            courseStat.ItemsSource = new string[] { "In Progress", "Completed", "Dropped", "Plan to Take" };
            courseStat.SelectedItem = _currentCourse.Status;
            courseDetails.Text = _currentCourse.CourseDetails;
            instructorName.Text = _currentCourse.InstructorName;
            instructorPhone.Text = _currentCourse.InstructorPhone;
            instructorEmail.Text = _currentCourse.InstructorEmail;
            optionalNotes.Text = _currentCourse.OptionalNotes;
        }

        private async void OnAssessmentButtonClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            int assessmentId = (int)button.CommandParameter;
            await NavigateToAssessment(assessmentId);
        }

        private async void OnAddAssessmentClicked(bool canAddPerformance, bool canAddObjective)
        {
            string typeToAdd = canAddPerformance ? "Performance" : (canAddObjective ? "Objective" : null);
            if (typeToAdd == null)
            {
                await DisplayAlert("Limit Reached", "You cannot add more than one Performance and one Objective assessment.", "OK");
                return;
            }

            var newAssessment = new Assessment
            {
                CourseId = _currentCourse.CourseId,
                Type = typeToAdd,
                Name = "New " + typeToAdd,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
            };

            _db.Insert(newAssessment);
            _assessments.Add(newAssessment);
            RefreshAssessmentButtons();
            await NavigateToAssessment(newAssessment.AssessmentId);
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            if (courseStart.Date >= courseEnd.Date)
            {
                DisplayAlert("Validation Error", "The start date must be before the end date.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(courseName.Text) ||
                string.IsNullOrWhiteSpace(instructorName.Text) ||
                string.IsNullOrWhiteSpace(instructorPhone.Text) ||
                string.IsNullOrWhiteSpace(instructorEmail.Text) ||
                !Regex.IsMatch(instructorEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                DisplayAlert("Invalid Input", "Please ensure all fields are filled correctly.", "OK");
                return;
            }

            SaveCourse();
        }
        private void OnStartAlertClicked(object sender, EventArgs e)
        {
            var notifyTime = _currentCourse.Start.Date.Add(new TimeSpan(8, 0, 0));
            if (notifyTime >= DateTime.Now)
            {
                var notification = new NotificationRequest
                {
                    NotificationId = _currentCourse.CourseId,
                    Title = "Course Start Alert",
                    Description = $"The course '{_currentCourse.CourseName}' is starting today!",
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
            var notifyTime = _currentCourse.End.Date.Add(new TimeSpan(17, 0, 0));
            if (notifyTime >= DateTime.Now)
            {
                var notification = new NotificationRequest
                {
                    NotificationId = _currentCourse.CourseId + 1000,
                    Title = "Course End Alert",
                    Description = $"The course '{_currentCourse.CourseName}' is ending today!",
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
        private async void OnShareNotesClicked(object sender, EventArgs e)
        {
            var notes = optionalNotes.Text?.Trim();
            if (string.IsNullOrWhiteSpace(notes))
            {
                await DisplayAlert("Error", "Please provide valid notes to share.", "OK");
                return;
            }

            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Share Course Notes",
                Text = notes
            });
        }
        private async Task NavigateToAssessment(int assessmentId)
        {
            var assessmentPage = new AssessmentView(assessmentId, _currentCourse.CourseId, LoadAssessments);
            await Navigation.PushAsync(assessmentPage);
        }

        private void OnStatusChanged(object sender, EventArgs e)
        {
            if (courseStat.SelectedItem != null)
            {
                _currentCourse.Status = courseStat.SelectedItem.ToString();
            }
        }

        public void SaveCourse()
        {
            if (ValidateInputs())
            {
                _currentCourse.CourseName = courseName.Text;
                _currentCourse.Start = courseStart.Date;
                _currentCourse.End = courseEnd.Date;
                _currentCourse.Status = courseStat.SelectedItem?.ToString() ?? "Unknown";
                _currentCourse.CourseDetails = courseDetails.Text;
                _currentCourse.InstructorName = instructorName.Text;
                _currentCourse.InstructorPhone = instructorPhone.Text;
                _currentCourse.InstructorEmail = instructorEmail.Text;
                _currentCourse.OptionalNotes = optionalNotes.Text;

                _db.InsertOrReplace(_currentCourse);
                DisplayAlert("Success", "Course details updated successfully.", "OK");
            }
        }

        private bool ValidateInputs()
        {
            if (courseStart.Date >= courseEnd.Date)
            {
                DisplayAlert("Validation Error", "The start date must be before the end date.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(courseName.Text) ||
                string.IsNullOrWhiteSpace(instructorName.Text) ||
                string.IsNullOrWhiteSpace(instructorPhone.Text) ||
                string.IsNullOrWhiteSpace(instructorEmail.Text) ||
                !Regex.IsMatch(instructorEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                DisplayAlert("Invalid Input", "Please ensure all fields are filled correctly.", "OK");
                return false;
            }
            return true;
        }
        private void OnDeleteClicked(object sender, EventArgs e)
        {
            DeleteCourse();
        }
        public void DeleteCourse()
        {
            _db.Delete(_currentCourse);
            DisplayAlert("Success", "Course deleted successfully.", "OK");
            Navigation.PopAsync();
        }
    }
}










