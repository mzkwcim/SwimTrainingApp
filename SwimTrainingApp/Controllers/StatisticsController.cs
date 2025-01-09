using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwimTrainingApp.Data;
using SwimTrainingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwimTrainingApp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public IActionResult TaskTypeDistribution(int? athleteId, DateTime? startDate, DateTime? endDate)
        {

            ViewBag.Athletes = _context.Users
                .Where(u => u.Role == UserRole.Athlete)
                .ToList(); 

            if (!athleteId.HasValue || !startDate.HasValue || !endDate.HasValue)
            {
                return View(new List<TaskStatsViewModel>());
            }

            var tasks = _context.TrainingTasks
                .Include(t => t.Training)
                .Where(t => t.Training.Date >= startDate && t.Training.Date <= endDate)
                .Where(t => _context.Attendances.Any(a => a.TrainingId == t.TrainingId && a.AthleteId == athleteId && a.IsPresent))
                .ToList();

            if (!tasks.Any())
            {
                return View(new List<TaskStatsViewModel>());
            }

            var totalTasks = tasks.Count;
            var distribution = tasks
                .GroupBy(t => t.TaskType)
                .Select(g => new TaskStatsViewModel
                {
                    TaskType = g.Key.ToString(),
                    TotalOccurrences = g.Count(),
                    Percentage = (g.Count() / (double)totalTasks) * 100
                })
                .ToList();

            return View(distribution);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public IActionResult AthleteTaskStats(int? athleteId, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Athletes = _context.Users
                .Where(u => u.Role == UserRole.Athlete)
                .ToList();

            if (!athleteId.HasValue || !startDate.HasValue || !endDate.HasValue)
            {
                return View(new AthleteTaskStatsViewModel
                {
                    AthleteId = athleteId ?? 0,
                    TotalDistance = 0,
                    TaskDistribution = new List<TaskTypeStats>()
                });
            }

            var attendedTrainings = _context.Attendances
                .Where(a => a.AthleteId == athleteId && a.IsPresent &&
                            a.Training.Date >= startDate && a.Training.Date <= endDate)
                .Select(a => a.TrainingId)
                .ToList();

            if (!attendedTrainings.Any())
            {
                return View(new AthleteTaskStatsViewModel
                {
                    AthleteId = athleteId.Value,
                    TotalDistance = 0,
                    TaskDistribution = new List<TaskTypeStats>()
                });
            }

            var tasks = _context.TrainingTasks
                .Where(t => attendedTrainings.Contains(t.TrainingId))
                .ToList();

            var totalDistance = tasks.Sum(t => t.Distance);

            var taskDistribution = tasks
                .GroupBy(t => t.TaskType)
                .Select(g => new TaskTypeStats
                {
                    TaskType = g.Key.ToString(),
                    TotalDistance = g.Sum(t => t.Distance),
                    Percentage = totalDistance > 0
                        ? Math.Round((g.Sum(t => t.Distance) / (double)totalDistance) * 100, 2)
                        : 0
                })
                .ToList();

            return View(new AthleteTaskStatsViewModel
            {
                AthleteId = athleteId.Value,
                TotalDistance = totalDistance,
                TaskDistribution = taskDistribution
            });
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public IActionResult TotalTaskStats()
        {
            var tasks = _context.TrainingTasks.ToList();

            if (!tasks.Any())
            {
                return View(new List<TaskTypeStats>());
            }

            var totalDistance = tasks.Sum(t => t.Distance);

            var taskStatistics = tasks
                .GroupBy(t => t.TaskType)
                .Select(g => new TaskTypeStats
                {
                    TaskType = g.Key.ToString(),
                    TotalDistance = g.Sum(t => t.Distance),
                    Percentage = (g.Sum(t => t.Distance) / (double)totalDistance) * 100
                })
                .ToList();

            ViewBag.TotalDistanceMeters = totalDistance;
            ViewBag.TotalDistanceKilometers = totalDistance / 1000.0;

            return View(taskStatistics);
        }

    }
}
