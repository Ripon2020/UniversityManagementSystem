using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using University.Models;

namespace University.Controllers
{
    public class CourseController : Controller
    {
        private UniversityDBContext db = new UniversityDBContext();
        
        public ActionResult Index()
        {
            return View();
        }
        
        // 3. Save Course Story
        public ActionResult Save()
        {
            //var DepartmentList = new List<string>();

            //var departments = from d in db.Departments
            //                  orderby d.DepartmentCode
            //                  select d.DepartmentCode;

            //DepartmentList.AddRange(departments.Distinct());

            //ViewBag.courseDepartment = new SelectList(DepartmentList);

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");

            ViewBag.SemesterId = new SelectList(db.Semesters, "SemesterId", "SemesterName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "CourseId,CourseCode,CourseName,CourseCredit,CourseDescription,DepartmentId,SemesterId")] Course course)
        {
            if (ModelState.IsValid)
            {
                db.Courses.Add(course);
                db.SaveChanges();

                ViewBag.Msg = "Course Saved Succesfully!";
            }

            //var DepartmentList = new List<string>();

            //var departments = from d in db.Departments
            //                  orderby d.DepartmentCode
            //                  select d.DepartmentCode;

            //DepartmentList.AddRange(departments.Distinct());

            //ViewBag.courseDepartment = new SelectList(DepartmentList);

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

            ViewBag.SemesterId = new SelectList(db.Semesters, "SemesterId", "SemesterName", course.SemesterId);

            return View(course);
        }

        //Data Annotation - Unique Check
        [HttpPost]
        public JsonResult DoesCourseCodeExist(string CourseCode)
        {
            return Json((!db.Courses.Any(x => x.CourseCode == CourseCode)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DoesCourseNameExist(string CourseName)
        {
            return Json((!db.Courses.Any(x => x.CourseName == CourseName)), JsonRequestBehavior.AllowGet);
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
