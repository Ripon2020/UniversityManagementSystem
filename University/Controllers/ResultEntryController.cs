using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Rotativa;
using University.Models;

namespace University.Controllers
{
    public class ResultEntryController : Controller
    {
        private UniversityDBContext db = new UniversityDBContext();

        public ActionResult Index()
        {
            return View();
        }

        // 11. Save Student Result Story
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseCode");

            ViewBag.GradeId = new SelectList(db.Grades, "GradeId", "Name");

            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "RegistrationId");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ResultEntryId,StudentId,CourseId,GradeId")] ResultEntry resultentry)
        {
            if (ModelState.IsValid)
            {
                var result = db.ResultEntries.Count(r => r.StudentId == resultentry.StudentId && r.CourseId == resultentry.CourseId) == 0;

                if (result)
                {
                    Grade aGrade = db.Grades.Where(g => g.GradeId == resultentry.GradeId).FirstOrDefault();

                    EnrollCourse resultEnrollCourse = db.EnrollCourses.FirstOrDefault(r => r.CourseId == resultentry.CourseId && r.StudentId == resultentry.StudentId);

                    if (resultEnrollCourse != null) 
                        resultEnrollCourse.GradeName = aGrade.Name;

                    db.ResultEntries.Add(resultentry);
                    db.SaveChanges();

                    TempData["success"] = "Result Successfully Entered";

                    return RedirectToAction("Create");
                }
                else
                {
                    TempData["Already"] = "Result Of This Course has Already Been Assigned";

                    return RedirectToAction("Create");
                }
            }

            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseCode", resultentry.CourseId);

            ViewBag.GradeId = new SelectList(db.Grades, "GradeId", "Name", resultentry.GradeId);

            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "RegistrationId", resultentry.StudentId);

            return View(resultentry);
        }

        public PartialViewResult StudentInfoLoad(int? studentId)
        {
            if (studentId != null)
            {
                Student aStudent = db.Students.FirstOrDefault(s => s.StudentId == studentId);
                ViewBag.Name = aStudent.StudentName;
                ViewBag.Email = aStudent.StudentEmail;
                ViewBag.Dept = aStudent.Department.DepartmentName;

                TempData["StudentInfo"] = aStudent;

                return PartialView("~/Views/Shared/_StudentInformation.cshtml");
            }
            else
            {
                return PartialView("~/Views/Shared/_StudentInformation.cshtml");
            }

        }

        public PartialViewResult CourseLoad(int? studentId)
        {
            List<EnrollCourse> enrollmentList = new List<EnrollCourse>();

            List<Course> courseList = new List<Course>();

            if (studentId != null)
            {
                enrollmentList = db.EnrollCourses.Where(e => e.StudentId == studentId).ToList();

                foreach (EnrollCourse anEnrollment in enrollmentList)
                {
                    Course aCourse = db.Courses.FirstOrDefault(c => c.CourseId == anEnrollment.CourseId);
                    courseList.Add(aCourse);
                }

                ViewBag.CourseId = new SelectList(courseList, "CourseId", "CourseName");
            }

            return PartialView("~/Views/shared/_FilteredCourse.cshtml");
        }

        // 12. View Result Story
        public ActionResult ViewResult()
        {
            ViewBag.StudentId = new SelectList(db.Students, "StudentId", "RegistrationId");

            return View();
        }

        public PartialViewResult ResultInfoLoad(int? studentId)
        {
            Session["ResultInfo"] = studentId;

            List<EnrollCourse> enrollCourseList = new List<EnrollCourse>();

            if (studentId != null)
            {
                enrollCourseList = db.EnrollCourses.Where(r => r.StudentId == studentId).ToList();

                if (enrollCourseList.Count == 0)
                {
                    Student aStudent = db.Students.Find(studentId);

                    ViewBag.NotEnrolled = aStudent.RegistrationId + "  : has not taken any course";
                }
                
            }

            return PartialView("~/Views/shared/_resultInformation.cshtml", enrollCourseList);
        }

        public ActionResult ResultPDF(int? studentId)
        {
            List<EnrollCourse> enrollCourseList = new List<EnrollCourse>();

            if (studentId != null)
            {
                enrollCourseList = db.EnrollCourses.Where(r => r.StudentId == studentId).ToList();

                Student aStudent = db.Students.Where(s => s.StudentId == studentId).FirstOrDefault();

                ViewBag.studentName = aStudent.StudentName;
                ViewBag.studentEmail = aStudent.StudentEmail;
                ViewBag.studentDepartment = aStudent.Department.DepartmentName;
                ViewBag.studentRegNo = aStudent.RegistrationId;

                if (enrollCourseList.Count == 0)
                {
                    //Student aStudent = db.Students.Find(studentId);

                    ViewBag.NotEnrolled = aStudent.RegistrationId + "  : has not taken any course";
                }

                var result = 0.0;

                foreach (var course in enrollCourseList)
                {
                    switch (course.GradeName)
                    {
                        case "A+":
                            result += 4.00;
                            break;
                        case "A":
                            result += 3.75;
                            break;
                        case "A-":
                            result += 3.50;
                            break;
                        case "B+":
                            result += 3.25;
                            break;
                        case "B":
                            result += 3.00;
                            break;
                        case "B-":
                            result += 2.75;
                            break;
                        case "C+":
                            result += 2.50;
                            break;
                        case "C":
                            result += 2.25;
                            break;
                        case "C-":
                            result += 2.00;
                            break;
                        case "D+":
                            result += 1.75;
                            break;
                        case "D":
                            result += 1.50;
                            break;
                        case "D-":
                            result += 1.25;
                            break;
                    }
                }

                result = result / enrollCourseList.Count;
                ViewBag.result = result.ToString("0.00");

            }
            return View(enrollCourseList);
        }

        public ActionResult ExportPDF()
        {
            var sId = (int)Session["ResultInfo"];

            return new ActionAsPdf("ResultPDF", new {studentId = sId})
            {
                FileName = Server.MapPath("~/Content/ListResults.pdf")
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
