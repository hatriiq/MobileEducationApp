using Microsoft.Maui.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static C971.Database;

namespace C971
{
    public partial class MainPage : ContentPage
    {
        public static List<Term> terms = new List<Term>();
        public static Dictionary<Term, List<Course>> courses = new Dictionary<Term, List<Course>>();
        public static Dictionary<int, Course> CourseList = new Dictionary<int, Course>();
        public static Term termSelected;
        public static List<string> statuses = new List<string>();
        public static string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "MyApp.db");

        public MainPage()
        {
            InitializeComponent();
            CreateDatabase();
            SyncDb();
            LoadUI(1);
            AddDummyData();
        }
        private void OnSaveClicked(object sender, EventArgs e)
        {
            if (termStart.Date >= termEnd.Date)
            {
                DisplayAlert("Validation Error", "The start date must be before the end date.", "OK");
                return;
            }

            
            termSelected.TermName = termTitle.Text;
            termSelected.Start = termStart.Date;
            termSelected.End = termEnd.Date;
           

            using (var db = new SQLiteConnection(databasePath: App.DatabasePath))
            {
                db.Update(termSelected);
            }

            LoadUI(1);
            UpdateTermUI(termSelected);


            DisplayAlert("Success", "Term details updated successfully.", "OK");
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            SyncDb();  
            LoadUI(termSelected.TermId);  
        }

        private void UpdateTermUI(Term updatedTerm)
        {

            termTitle.Text = updatedTerm.TermName;
            
        }
        private void OnTermDelete()
        {
            if (termSelected == null)
            {
                DisplayAlert("Error", "No term selected to delete.", "OK");
                return;
            }
            SaveCurrentChanges();
            using var db = new SQLiteConnection(DatabasePath);
            db.Delete(termSelected);
            SyncDb(); 
            LoadUI(terms.Any() ? terms[0].TermId : 1);  
        }
        public void AddDummyData()
        {
            using var db = new SQLiteConnection(DatabasePath);
            db.CreateTable<Term>(); 

            
            var existingTerms = db.Table<Term>().Count();
            if (existingTerms == 0)
            {
                Term term1 = new Term("Spring 2024", DateTime.Now, DateTime.Now.AddMonths(6));
                Term term2 = new Term("Fall 2024", DateTime.Now.AddMonths(6), DateTime.Now.AddMonths(12));
                db.Insert(term1);
                db.Insert(term2);

                CreateCoursesForTerm(db, term1, "Spring");
                CreateCoursesForTerm(db, term2, "Fall");
            }
        }

       
        private void OnNewTerm()
        {
            SaveCurrentChanges();
            using var db = new SQLiteConnection(DatabasePath);
            var latestTerm = db.Table<Term>().OrderByDescending(t => t.TermId).FirstOrDefault();
            int nextTermId = latestTerm != null ? latestTerm.TermId + 1 : 1;
            string termName = "Term " + nextTermId;
            Term newTerm = new Term(termName, DateTime.Now, DateTime.Now.AddMonths(6));
            db.Insert(newTerm);

            SyncDb();  
            LoadUI(nextTermId);  
        }
        private void SaveCurrentChanges()
        {
            if (termSelected != null)
            {
                termSelected.TermName = termTitle.Text;
                termSelected.Start = termStart.Date;
                termSelected.End = termEnd.Date;

                using var db = new SQLiteConnection(DatabasePath);
                db.Update(termSelected);
            }
        }
        private void CreateCoursesForTerm(SQLiteConnection db, Term term, string season)
        {
            string[] courseTitles;
            string[] courseDescriptions;

            if (season == "Spring")
            {
                courseTitles = new[]
                {
            "Introduction to Automotive Technology",
            "Automotive Electrical Systems",
            "Engine Repair Techniques",
            "HVAC for Vehicles",
            "Automotive Suspension and Steering",
            "Braking Systems"
        };

                courseDescriptions = new[]
                {
            "Introductory course on automotive principles.",
            "Fundamentals of vehicle electronics and circuit theory.",
            "Hands-on engine repair methods.",
            "Heating, ventilation, and air conditioning systems in vehicles.",
            "Study of suspension and steering systems design and maintenance.",
            "Comprehensive overview of automotive braking systems."
        };
            }
            else
            {
                courseTitles = new[]
                {
            "Advanced Automotive Diagnostics",
            "Performance Engine Tuning",
            "Electric Vehicle Technology",
            "Automotive Design and Engineering",
            "Automotive Safety Systems",
            "Automotive Manufacturing Processes"
        };

                courseDescriptions = new[]
                {
            "Techniques for diagnosing complex automotive issues.",
            "Strategies for tuning engines for optimal performance.",
            "Overview of electric vehicle components and systems.",
            "Principles of automotive design and engineering.",
            "Examination of safety systems in modern vehicles.",
            "Insights into automotive manufacturing and assembly."
        };
            }

            for (int i = 0; i < courseTitles.Length; i++)
            {
                var course = new Course
                (
                    term.TermId,
                    courseTitles[i],
                    term.Start.AddMonths(i), 
                    term.Start.AddMonths(i + 1), 
                    "In Progress",
                    courseDescriptions[i],
                    "Anika Patel",
                    "555-123-4567",
                    "anika.patel@strimeuniversity.edu",
                    "PA",
                    "OA",
                    ""
                );
                Database.AddCourse(db, course);
                var performanceAssessment = new Assessment
                {
                    CourseId = course.CourseId,
                    Name = $"{course.CourseName} - Performance Assessment",
                    StartDate = course.Start,
                    EndDate = course.End,
                    Type = "Performance"
                };
                var objectiveAssessment = new Assessment
                {
                    CourseId = course.CourseId,
                    Name = $"{course.CourseName} - Objective Assessment",
                    StartDate = course.Start,
                    EndDate = course.End,
                    Type = "Objective"
                };
                db.Insert(performanceAssessment);
                db.Insert(objectiveAssessment);
            }
        }


        public void LoadUI(int termIndex)
        {
            termBind.Children.Clear();
            courseBind.Children.Clear();

            if (!terms.Any())
            {
                return;
            }

            termIndex = Math.Max(1, Math.Min(termIndex, terms.Count));
            termSelected = terms[termIndex - 1];

            
            foreach (Term temporaryTerm in terms)
            {
                Button button = new Button
                {
                    Text = temporaryTerm.TermName,
                    Padding = 5,
                    BackgroundColor = Colors.Black,
                    TextColor = Colors.White,
                    CornerRadius = 5,
                };
                button.Clicked += (sender, args) => LoadUI(temporaryTerm.TermId);
                termBind.Children.Add(button);
            }

       
            Button buttonAddTerm = new Button()
            {
                Text = "Add Term",
                Padding = 5,
                BackgroundColor = Colors.Black,
                TextColor = Colors.White,
                CornerRadius = 5,
            };
            buttonAddTerm.Clicked += (sender, args) => OnNewTerm();
            termBind.Children.Add(buttonAddTerm);

            
            foreach (Course course in courses[termSelected])
            {
                Grid grid = new Grid
                {
                    BackgroundColor = Colors.Black
                };
                Button courseButton = new Button
                {
                    Text = course.CourseName
                };
                courseButton.Clicked += async (sender, args) => await Navigation.PushAsync(new CourseView(course.CourseId));
                grid.Children.Add(courseButton);

                SwipeItem deleteItem = new SwipeItem
                {
                    Text = "Delete",
                    BindingContext = course,
                    BackgroundColor = Colors.White
                };
                deleteItem.Invoked += OnDeleteInvoked;
                SwipeItems swipeItems = new SwipeItems { deleteItem };
                SwipeView swipeView = new SwipeView
                {
                    RightItems = swipeItems,
                    Content = grid
                };
                courseBind.Children.Add(swipeView);
            }

            
            if (courses[termSelected].Count < 6)
            {
                Button buttonCourseAdd = new Button
                {
                    Text = "Add Course",
                    BackgroundColor = Colors.Black,
                    TextColor = Colors.White,
                };
                buttonCourseAdd.Clicked += (sender, args) => OnNewCourse();
                courseBind.Children.Add(buttonCourseAdd);
            }

            
            if (courses[termSelected].Count == 0)
            {
                Button buttonTermRemove = new Button
                {
                    Text = "Delete Term",
                    BackgroundColor = Colors.Red,
                    TextColor = Colors.White,
                };
                buttonTermRemove.Clicked += (sender, args) => OnTermDelete();
                courseBind.Children.Add(buttonTermRemove);
            }

            
            termStart.Date = termSelected.Start;
            termEnd.Date = termSelected.End;
            termTitle.Text = termSelected.TermName;
        }


        private void OnNewCourse()
        {
            
            var newCourse = new Course
            {
                TermId = termSelected.TermId,
                CourseName = "New Course",
                Start = DateTime.Now,
                End = DateTime.Now.AddMonths(1),
                Status = "In Progress",
                CourseDetails = "No details"
            };

           
            using var db = new SQLiteConnection(DatabasePath);
            db.Insert(newCourse);

           
            SyncDb();

            
            Navigation.PushAsync(new CourseView(newCourse.CourseId));
        }

        private async Task<DateTime> DisplayDatePromptAsync(string title, string message)
        {
            string dateString = await DisplayPromptAsync(title, message, "OK", "Cancel", "YYYY-MM-DD", maxLength: 10, keyboard: Keyboard.Numeric);

            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date;
            }
            else
            {
                await DisplayAlert("Invalid Input", "Please enter a valid date.", "OK");
                return await DisplayDatePromptAsync(title, message);
            }
        }

        private async Task OnNewCourse1()
        {
            string courseName = await DisplayPromptAsync("New Course", "Enter course name:");
            if (string.IsNullOrWhiteSpace(courseName))
            {
                await DisplayAlert("Invalid Input", "Course name cannot be empty.", "OK");
                return;
            }

            DateTime courseStart = await DisplayDatePromptAsync("Course Start Date", "Select the start date for the course:");
            DateTime courseEnd = await DisplayDatePromptAsync("Course End Date", "Select the end date for the course:");

            if (courseEnd < courseStart)
            {
                await DisplayAlert("Invalid Dates", "End date cannot be earlier than start date.", "OK");
                return;
            }

            string[] statuses = { "In Progress", "Completed", "Dropped", "Plan to Take" };
            string status = await DisplayActionSheet("Select Course Status", null, null, statuses);
            if (string.IsNullOrWhiteSpace(status))
            {
                await DisplayAlert("Invalid Input", "You must select a status.", "OK");
                return;
            }

            string courseDetails = await DisplayPromptAsync("Course Details", "Enter details for the course:");
            if (string.IsNullOrWhiteSpace(courseDetails))
            {
                courseDetails = "No details provided.";
            }

            Course newCourse = new Course(termSelected.TermId, courseName, courseStart, courseEnd, status, courseDetails, "Anika Patel", "555-123-4567", "anika.patel@strimeuniversity.edu", "PA", "OA", "");
            using (var db = new SQLiteConnection(MainPage.DatabasePath))
            {
                Database.AddCourse(db, newCourse);
            }

            MainPage.SyncDb();
            LoadUI(termSelected.TermId);
            await DisplayAlert("Success", "New course added successfully.", "OK");
        }

        private void OnDeleteInvoked(object sender, EventArgs e)
        {
            var item = sender as SwipeItem;
            var course = item.BindingContext as Course;
            Database.DeleteCourse(course);
            MainPage.SyncDb();
            LoadUI(termSelected.TermId);
        }

       

        private void TermEnd_DateSelected(object sender, DateChangedEventArgs e)
        {
            using var db = new SQLiteConnection(DatabasePath);
            if (CheckDates(termStart.Date, termEnd.Date))
            {
                termEnd.Date = e.NewDate;
                termSelected.End = e.NewDate;
            }
        }

        private void TermStart_DateSelected(object sender, DateChangedEventArgs e)
        {
            using var db = new SQLiteConnection(DatabasePath);
            if (CheckDates(termStart.Date, termEnd.Date))
            {
                termStart.Date = e.NewDate;
                termSelected.Start = e.NewDate;
            }
        }

        public static bool CheckDates(DateTime start, DateTime end)
        {
            return end >= start;
        }

       

        private void TermTitleChange(object sender, TextChangedEventArgs e)
        {
            using var db = new SQLiteConnection(DatabasePath);
            if (e.NewTextValue != null)
            {
                termSelected.TermName = e.NewTextValue;
                LoadUI(termSelected.TermId);
            }
        }

        public static void SyncDb()
        {
            terms.Clear();
            courses.Clear();
            CourseList.Clear();
            using var db = new SQLiteConnection(DatabasePath);
            var temporaryTerm = db.Query<Term>("SELECT * FROM Terms");
            foreach (Term term in temporaryTerm)
            {
                terms.Add(term);
            }

            foreach (Term term in terms)
            {
                var temporaryCourses = db.Query<Course>($"SELECT * FROM Courses WHERE termId={term.TermId}");
                List<Course> CourseList = new List<Course>();
                foreach (Course course in temporaryCourses)
                {
                    CourseList.Add(course);
                    MainPage.CourseList[course.CourseId] = course;
                }
                courses.Add(term, CourseList);
            }
        }

        private static void CreateDatabase()
        {
            using var db = new SQLiteConnection(DatabasePath);
            db.CreateTable<Term>();
        }
    }
}
