using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using University.Models;

namespace University.Controllers
{
    public class TeacherController : Controller
    {
        private UniversityDBContext db = new UniversityDBContext();
        
        public ActionResult Index()
        {
            return View();
        }

        // 4. Save Teacher Story
        public ActionResult Save()
        {
            //var DepartmentList = new List<string>();

            //var departments = from d in db.Departments
            //                  orderby d.DepartmentCode
            //                  select d.DepartmentCode;

            //DepartmentList.AddRange(departments.Distinct());

            //ViewBag.teacherDepartment = new SelectList(DepartmentList);

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");

            //var DesignationList = new List<string>();

            //var designations = from d in db.Designations
            //                   orderby d.DesignationName
            //                   select d.DesignationName;

            //DesignationList.AddRange(designations.Distinct());

            //ViewBag.teacherDesignation = new SelectList(DesignationList);

            ViewBag.DesignationId = new SelectList(db.Designations, "DesignationId", "DesignationName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "TeacherId,TeacherName,TeacherAddress,TeacherEmail,TeacherContactNo,DesignationId,DepartmentId,CreditToBeTaken")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();

                ViewBag.Msg = "Teacher Saved Succesfully!";
            }

            //var DepartmentList = new List<string>();

            //var departments = from d in db.Departments
            //                  orderby d.DepartmentCode
            //                  select d.DepartmentCode;

            //DepartmentList.AddRange(departments.Distinct());

            //ViewBag.teacherDepartment = new SelectList(DepartmentList);

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", teacher.DepartmentId);

            //var DesignationList = new List<string>();

            //var designations = from d in db.Designations
            //                  orderby d.DesignationName
            //                  select d.DesignationName;

            //DesignationList.AddRange(designations.Distinct());

            //ViewBag.teacherDesignation = new SelectList(DesignationList);

            ViewBag.DesignationId = new SelectList(db.Designations, "DesignationId", "DesignationName", teacher.DesignationId);

            return View(teacher);
        }

        //Data Annotation - Unique Check
        [HttpPost]
        public JsonResult DoesTeacherEmailExist(string TeacherEmail)
        {
            return Json((!db.Teachers.Any(x => x.TeacherEmail == TeacherEmail)), JsonRequestBehavior.AllowGet);
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
