﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Mvc;
using University.Models;

namespace University.Controllers
{
    public class RoomAllocationController : Controller
    {
        private UniversityDBContext db = new UniversityDBContext();

        public ActionResult Index()
        {
            return View();
        }

        // 8. Allocate Classrooms Story
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseCode");

            ViewBag.DayId = new SelectList(db.Days, "DayId", "Name");

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");

            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="RoomAllocationId,DepartmentId,CourseId,RoomId,DayId,StartTime,EndTime")] RoomAllocation roomallocation)
        {
            Room room = db.Rooms.Find(roomallocation.RoomId);

            Course course = db.Courses.Find(roomallocation.CourseId);

            Day day = db.Days.Find(roomallocation.DayId);

            if (ModelState.IsValid)
            {
                int givenfrom, givenEnd;

                try
                {
                    givenfrom = ConvertTimetoInt(roomallocation.StartTime);
                }
                catch (Exception anException)
                {
                    if (TempData["ErrorMessage3"] == null)
                    {
                        TempData["ErrorMessage1"] = anException.Message;
                    }

                    TempData["ErrorMessage4"] = TempData["ErrorMessage3"];

                    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

                    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.DepartmentId == course.DepartmentId), "CourseId", "CourseCode", roomallocation.CourseId);

                    ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

                    ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

                    return View(roomallocation);
                }

                try
                {
                    givenEnd = ConvertTimetoInt(roomallocation.EndTime);
                }
                catch (Exception anException)
                {
                    if (TempData["ErrorMessage3"] == null)
                    {
                        TempData["ErrorMessage2"] = anException.Message;
                    }

                    TempData["ErrorMessage5"] = TempData["ErrorMessage3"];

                    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

                    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.DepartmentId == course.DepartmentId), "CourseId", "CourseCode", roomallocation.CourseId);

                    ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

                    ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

                    return View(roomallocation);
                }

                if (givenEnd <= givenfrom)
                {
                    TempData["Message6"] = "Class Should be Started Before End (24 hours)";

                    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

                    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.DepartmentId == course.DepartmentId), "CourseId", "CourseCode", roomallocation.CourseId);

                    ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

                    ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

                    return View(roomallocation);
                }

                //if (givenEnd == givenfrom)
                //{
                //    TempData["Message"] = "Class Should Start Before End (24 hours)";

                //    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

                //    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.DepartmentId == course.DepartmentId), "CourseId", "CourseCode", roomallocation.CourseId);

                //    ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

                //    ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

                //    return View(roomallocation);
                //}

                if ((givenfrom < 0) || (givenEnd >= (24 * 60)))
                {
                    TempData["Message7"] = " 24 hours--format HH:MM";

                    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

                    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.DepartmentId == course.DepartmentId), "CourseId", "CourseCode", roomallocation.CourseId);

                    ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

                    ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

                    return View(roomallocation);
                }

                List<RoomAllocation> overLapList = new List<RoomAllocation>();

                var DayRoomAllocationList = db.RoomAllocations.Include(c => c.Course).Include(d => d.Day).Include(r => r.Room).Where(r => r.RoomId == roomallocation.RoomId && r.DayId == roomallocation.DayId && r.Status == "Allocated").ToList();

                foreach (var aDayroomAllocation in DayRoomAllocationList)
                {
                    int DbFrom = ConvertTimetoInt(aDayroomAllocation.StartTime);

                    int DbEnd = ConvertTimetoInt(aDayroomAllocation.EndTime);

                    if (((DbFrom <= givenfrom) && (givenfrom < DbEnd)) || ((DbFrom < givenEnd) && (givenEnd <= DbEnd)) || ((givenfrom <= DbFrom) && (DbEnd <= givenEnd)))
                    {
                        overLapList.Add(aDayroomAllocation);
                    }
                }

                if (overLapList.Count == 0)
                {
                    roomallocation.Status = "Allocated";

                    db.RoomAllocations.Add(roomallocation);
                    db.SaveChanges();

                    TempData["Message8"] = "Room : " + room.Name + " is Allocated for course : " + course.CourseCode + " From " + roomallocation.StartTime + " to " + roomallocation.EndTime + " on " + day.Name;
                }
                else
                {
                    string message = "Room : " + room.Name + " is Overlapped With : ";

                    foreach (var anOverlappedRoom in overLapList)
                    {
                        int dbFrom = ConvertTimetoInt(anOverlappedRoom.StartTime);

                        int dbEnd = ConvertTimetoInt(anOverlappedRoom.EndTime);

                        string overlapStart, overlapEnd;

                        if ((dbFrom <= givenfrom) && (givenfrom < dbEnd))
                            overlapStart = roomallocation.StartTime;
                        else
                            overlapStart = anOverlappedRoom.StartTime;

                        if ((dbFrom < givenEnd) && (givenEnd <= dbEnd))
                            overlapEnd = roomallocation.EndTime;
                        else
                            overlapEnd = anOverlappedRoom.EndTime;

                        message += " Course : " + anOverlappedRoom.Course.CourseCode + " Start Time : " + anOverlappedRoom.StartTime + " End Time : " + anOverlappedRoom.EndTime + " Overlapped From : " + overlapStart + " To " + overlapEnd;
                    }

                    TempData["Message9"] = message + " on " + day.Name;

                    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");

                    ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", course.DepartmentId);

                    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.DepartmentId == course.DepartmentId), "CourseId", "CourseCode", roomallocation.CourseId);

                    ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

                    ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

                    return View(roomallocation);
                }

                return RedirectToAction("Create");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseCode", roomallocation.CourseId);

            ViewBag.DayId = new SelectList(db.Days, "DayId", "Name", roomallocation.DayId);

            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode", roomallocation.DepartmentId);

            ViewBag.RoomId = new SelectList(db.Rooms, "RoomId", "Name", roomallocation.RoomId);

            return View(roomallocation);
        }

        private int ConvertTimetoInt(string timeStr)
        {
            try
            {
                if (TimeFormateCheck(timeStr))
                {
                    if (timeStr.Length == 4)
                    {
                        timeStr = "0" + timeStr;
                    }

                    string hour = timeStr[0].ToString() + timeStr[1];

                    string minute = timeStr[3].ToString() + timeStr[4];

                    if (Convert.ToInt32(minute) > 59 || Convert.ToInt32(minute) < 0)
                    {
                        TempData["ErrorMessage3"] = "Minites Must Be Equal Or Less Then 59";

                        throw new Exception();
                    }

                    int time = (Convert.ToInt32(hour) * 60);

                    time += Convert.ToInt32(minute);

                    return time;
                }
                else
                {
                    throw new Exception("24 hours--format HH:MM");
                }
            }

            catch (FormatException aFormatException)
            {
                throw new FormatException("24 hours--format HH:MM", aFormatException);
            }

            catch (IndexOutOfRangeException aRangException)
            {
                throw new IndexOutOfRangeException("24 hours--format HH:MM", aRangException);
            }

            catch (Exception anException)
            {
                throw new Exception("24 hours--format HH:MM", anException);
            }
        }

        private bool TimeFormateCheck(string timeStr)
        {
            char[] list = timeStr.ToCharArray();
            foreach (var aChar in list)
            {
                if (aChar == ':')
                {
                    return true;
                }
            }
            return false;
        }

        public ActionResult LoadCourse(int? departmentId)
        {
            var courseList = db.Courses.Where(u => u.DepartmentId == departmentId).ToList();

            ViewBag.CourseId = new SelectList(courseList, "CourseId", "CourseName");

            return PartialView("~/Views/shared/_FilteredCourse.cshtml");

        }

        // 9. View Class Schedule and Room Allocation Information Story
        public ActionResult RoomAllocationView()
        {
            ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentCode");

            //List<Course> CourseList = db.Courses.ToList();

            //foreach (var aCourse in CourseList)
            //{
            //    aCourse.RoomAllocationList = db.RoomAllocations.Include(a => a.Room).Include(a => a.Day).Where(a => a.CourseId == aCourse.CourseId && a.Status == "Allocated").ToList();
            //}

            return View();
        }

        public PartialViewResult AllocatedRoomLoad(int? departmentId)
        {
            List<Course> courseList = null;

            if (departmentId != null)
            {
                courseList = db.Courses.Where(c => c.DepartmentId == departmentId).ToList();
            }

            foreach (var aCourse in courseList)
            {
                aCourse.RoomAllocationList = db.RoomAllocations.Include(a => a.Room).Include(a => a.Day).Where(a => a.CourseId == aCourse.CourseId && a.Status == "Allocated").ToList();
            }

            if (courseList.Count == 0)
            {
                ViewBag.NoCourse = "Department Empty";
            }

            return PartialView("~/Views/shared/_RoomAllocationView.cshtml", courseList);
        }

        //14.Unallocate All Classrooms Story
        public ActionResult UnallocateRoom()
        {
            return View();
        }

        public JsonResult UnallocateAllRooms()
        {
            var roomInfo = db.RoomAllocations.Where(s => s.Status == "Allocated").ToList();
            if (roomInfo.Count == 0)
            {
                return Json(false);
            }
            else
            {
                foreach (var room in roomInfo)
                {
                    room.Status = null;
                    db.RoomAllocations.AddOrUpdate(room);
                    db.SaveChanges();
                }
                return Json(true);
            }
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
