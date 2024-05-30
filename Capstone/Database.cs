using SQLite;
using System;
using System.IO;
using System.Linq;
using Microsoft.Maui.Storage;

namespace C971
{
    public class Database  //Encapsulation example
    {
        
        public static string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "MyApp.db");

        [Table("Terms")]
        public class Term
        {
            [PrimaryKey, AutoIncrement]
            public int TermId { get; set; }
            public string TermName { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public Term() { }

            public Term(string termName, DateTime start, DateTime end)
            {
                TermName = termName;
                Start = start;
                End = end;
            }
        }

        [Table("Courses")]
        public class Course
        {
            [PrimaryKey, AutoIncrement]
            public int CourseId { get; set; }
            public int TermId { get; set; }
            public string CourseName { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public string Status { get; set; }
            public string CourseDetails { get; set; }
            public string InstructorName { get; set; }
            public string InstructorPhone {  get; set; }
            public string InstructorEmail { get; set; }
            public string PerformanceAssessment { get; set; }
            public string ObjectiveAssessment { get; set; }
            public string OptionalNotes { get; set; }
            public Course() { }

            public Course(int termId, string courseName, DateTime start, DateTime end, string status, string courseDetails, string instructorName, string instructorPhone, string instructorEmail, string performanceAssessment, string objectiveAssessment, string optionalNotes)
            {
                TermId = termId;
                CourseName = courseName;
                Start = start;
                End = end;
                Status = status;
                CourseDetails = courseDetails;
                InstructorName = instructorName;
                InstructorPhone = instructorPhone;
                InstructorEmail = instructorEmail;
                PerformanceAssessment = performanceAssessment;
                ObjectiveAssessment = objectiveAssessment;
                OptionalNotes = optionalNotes;
            }
        }
        [Table("Assessments")]
        public class Assessment
        {
            [PrimaryKey, AutoIncrement]
            public int AssessmentId { get; set; }

            public int CourseId { get; set; }

            public string Name { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public string Type { get; set; } 

            public Assessment() { }

            public Assessment(int assessmentId,int courseId, string name, DateTime startDate, DateTime endDate, string type)
            {
                AssessmentId = assessmentId;
                CourseId = courseId;
                Name = name;
                StartDate = startDate;
                EndDate = endDate;
                Type = type;
            }
        }
        public static void CreateTable()
        {
            using var db = new SQLiteConnection(DatabasePath);
            db.CreateTable<Term>();
            db.CreateTable<Course>();
            db.CreateTable<Assessment>();
        }
        public static void AddAssessment(SQLiteConnection db, Assessment assessment) => db.Insert(assessment);  //  Add data

        public static void UpdateAssessment(SQLiteConnection db, Assessment assessment) => db.Update(assessment);  //  Modify data

        public static void DeleteAssessment(SQLiteConnection db, Assessment assessment) => db.Delete(assessment);  //  Delete data

        public static void AddNewCourse(int termId)
        {
            using var db = new SQLiteConnection(DatabasePath);
            Course newCourse = new Course(termId, "NewCourseAuto", DateTime.Now, DateTime.Now.AddMonths(4), "In Progress", "Course Details:", "Anika Patel", "555-123-4567", "anika.patel@strimeuniversity.edu", "PA", "OA","");
            AddCourse(db, newCourse); // Polymorphism example Overriding Course object with Method.
            db.Update(newCourse);
            MainPage.SyncDb();
        }

        public static void AddNewTerm()
        {
            using var db = new SQLiteConnection(DatabasePath);
            var latestTerm = db.Table<Term>().OrderByDescending(t => t.TermId).FirstOrDefault();
            int nextTermId = latestTerm != null ? latestTerm.TermId + 1 : 1;
            string termName = "Term " + nextTermId;
            Term newTerm = new Term(termName, DateTime.Now, DateTime.Now.AddDays(30));
            db.Insert(newTerm);
        }

        public static void DeleteCourse(Course course)
        {
            using var db = new SQLiteConnection(DatabasePath);
            db.Delete(course);
        }

        public static void AddTerm(SQLiteConnection db, Term term) => db.Insert(term);

        public static void UpdateTerm(SQLiteConnection db, Term term) => db.Update(term);

        public static void AddCourse(SQLiteConnection db, Course course) => db.Insert(course);

        public static void UpdateCourse(SQLiteConnection db, Course course) => db.Update(course);
        
    }
}
