﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult Index()
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            return View();
        }
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
        public IActionResult Edit(int? TrainingId)
        {
            ViewBag.Trainings = _context.Trainings.ToList(); 

            if (TrainingId == null)
            {
                return View(); 
            }

            var attendances = _context.Attendances.Where(a => a.TrainingId == TrainingId).ToList();

            if (attendances.Count == 0)
            {
                ViewBag.Message = "No attendance records found for this training.";
            }

            ViewBag.SelectedTrainingId = TrainingId;
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList(); 

            return View(attendances);
        }

        [HttpPost]
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
                return RedirectToAction("Index"); 
            }

            ViewBag.Trainings = _context.Trainings.ToList();
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Athlete).ToList();
            ViewBag.SelectedTrainingId = TrainingId;

            return View(Attendances); 
        }

        [HttpGet]
        public IActionResult ViewByDateRange(DateTime? startDate, DateTime? endDate)
        {
            var trainings = _context.Trainings
                .Where(t => (!startDate.HasValue || t.Date >= startDate) &&
                            (!endDate.HasValue || t.Date <= endDate))
                .ToList();

            if (!trainings.Any())
            {
                ViewBag.Message = "No trainings found in the selected date range.";
                return View(new List<AttendanceStatsViewModel>());
            }

            var trainingIds = trainings.Select(t => t.Id).ToList();

            var attendances = _context.Attendances
                .Where(a => trainingIds.Contains(a.TrainingId))
                .ToList();

            var athletes = _context.Users
                .Where(u => u.Role == UserRole.Athlete)
                .ToList();

            var attendanceStats = athletes.Select(athlete =>
            {
                var athleteAttendances = attendances.Where(a => a.AthleteId == athlete.Id).ToList();
                int totalTrainings = trainings.Count;
                int presentCount = athleteAttendances.Count(a => a.IsPresent);

                return new AttendanceStatsViewModel
                {
                    Athlete = athlete,
                    TotalTrainings = totalTrainings,
                    PresentCount = presentCount,
                    Percentage = totalTrainings > 0
                        ? Math.Round((double)presentCount / totalTrainings * 100, 2)
                        : 0
                };
            }).ToList();

            return View(attendanceStats);
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
