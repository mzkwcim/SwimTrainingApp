using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;

namespace SwimTrainingApp.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // Index - lista linków
        public IActionResult Index()
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            return View();
        }

        // Widok tworzenia listy obecności
        public IActionResult Create(int? TrainingId)
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList();

            if (TrainingId == null)
            {
                return View();
            }

            var users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList();
            var attendanceList = users.Select(user => new Attendance
            {
                TrainingId = TrainingId.Value,
                AthleteId = user.Id,
                IsPresent = false
            }).ToList();

            ViewBag.SelectedTraining = _context.Trainings.FirstOrDefault(t => t.Id == TrainingId);

            return View(attendanceList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(List<Attendance> attendances)
        {
            if (ModelState.IsValid)
            {
                foreach (var attendance in attendances)
                {
                    _context.Attendances.Add(attendance);
                }
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(attendances);
        }

        // Szczegóły obecności
        public IActionResult Details(int? TrainingId)
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList();

            if (TrainingId == null)
            {
                return View(new List<Attendance>());
            }

            var attendanceList = _context.Attendances
                .Where(a => a.TrainingId == TrainingId)
                .ToList();

            ViewBag.SelectedTraining = _context.Trainings.FirstOrDefault(t => t.Id == TrainingId);

            return View(attendanceList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int TrainingId, List<Attendance> Attendances)
        {
            if (ModelState.IsValid)
            {
                foreach (var attendance in Attendances)
                {
                    var existingAttendance = _context.Attendances.FirstOrDefault(a => a.Id == attendance.Id);

                    if (existingAttendance != null)
                    {
                        existingAttendance.IsPresent = attendance.IsPresent;
                    }
                }

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Trainings = _context.Trainings.ToList();
            ViewBag.SelectedTraining = _context.Trainings.FirstOrDefault(t => t.Id == TrainingId);
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList();

            return View(Attendances);
        }

        public IActionResult Delete(int? TrainingId)
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList();

            if (TrainingId == null)
            {
                return View(new List<Attendance>());
            }

            var attendanceList = _context.Attendances
                .Where(a => a.TrainingId == TrainingId)
                .ToList();

            ViewBag.SelectedTraining = _context.Trainings.FirstOrDefault(t => t.Id == TrainingId);

            return View(attendanceList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int TrainingId)
        {
            var attendanceList = _context.Attendances.Where(a => a.TrainingId == TrainingId).ToList();

            if (attendanceList.Any())
            {
                _context.Attendances.RemoveRange(attendanceList);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }


    }

}
