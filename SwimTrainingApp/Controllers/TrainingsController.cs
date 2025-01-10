using System;
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
    public class TrainingsController : Controller
    {
        private readonly AppDbContext _context;

        public TrainingsController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public async Task<IActionResult> Index()
        {
            var trainings = await _context.Trainings.ToListAsync();

            return View(trainings);
        }
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public async Task<IActionResult> Details(int? id)
        {
            var trainings = await _context.Trainings.ToListAsync();

            if (id == null)
            {
                ViewBag.Trainings = trainings; 
                return View();
            }

            var training = await _context.Trainings
                .Include(t => t.Tasks) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            ViewBag.Trainings = trainings; 
            return View(training); 
        }
        [Authorize(Roles = "Admin,Coach")]
        public IActionResult Create()
        {
            var model = new Training
            {
                Tasks = new List<TrainingTask> { new TrainingTask() } 
            };

            ViewBag.Message = TempData["Message"];
            ViewBag.IsSuccess = TempData["IsSuccess"];
            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Coach")]
        public async Task<IActionResult> Create([FromBody] Training training)
        {
            if (training == null)
            {
                Console.WriteLine("Training object is null.");
                return BadRequest("Training object is null.");
            }

            Console.WriteLine($"Received Date: {training.Date}");

            if (training.Tasks != null)
            {
                Console.WriteLine($"Number of Tasks: {training.Tasks.Count}");
                foreach (var task in training.Tasks)
                {
                    Console.WriteLine($"Task - Description: {task.TaskDescription}, Distance: {task.Distance}");
                }
            }

            try
            {
                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Training created successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the training.");
            }
        }
        [Authorize(Roles = "Admin,Coach")]
        public async Task<IActionResult> Edit(int? id)
        {
            var trainings = await _context.Trainings.ToListAsync();
            ViewBag.Trainings = trainings;

            if (id == null)
            {
                return View();
            }

            var training = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            Console.WriteLine($"Training ID: {training.Id}, Date: {training.Date}");
            foreach (var task in training.Tasks)
            {
                Console.WriteLine($"Task ID: {task.Id}, Section: {task.TrainingSection}, Description: {task.TaskDescription}, Distance: {task.Distance}, Type: {task.TaskType}");
            }

            return View(training);
        }

        [HttpPatch]
        [Authorize(Roles = "Admin,Coach")]
        public async Task<IActionResult> Edit(int id, [FromBody] Training updatedTraining)
        {
            if (id != updatedTraining.Id)
            {
                return BadRequest("Training ID mismatch.");
            }

            var existingTraining = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTraining == null)
            {
                return NotFound("Training not found.");
            }

            existingTraining.Date = updatedTraining.Date;

            foreach (var updatedTask in updatedTraining.Tasks)
            {
                var existingTask = existingTraining.Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);

                if (existingTask != null)
                {
                    existingTask.TrainingSection = updatedTask.TrainingSection;
                    existingTask.TaskDescription = updatedTask.TaskDescription;
                    existingTask.Distance = updatedTask.Distance;
                    existingTask.TaskType = updatedTask.TaskType;
                }
                else
                {
                    existingTraining.Tasks.Add(new TrainingTask
                    {
                        TrainingSection = updatedTask.TrainingSection,
                        TaskDescription = updatedTask.TaskDescription,
                        Distance = updatedTask.Distance,
                        TaskType = updatedTask.TaskType,
                        TrainingId = existingTraining.Id
                    });
                }
            }

            var tasksToRemove = existingTraining.Tasks
                .Where(existingTask => !updatedTraining.Tasks.Any(updatedTask => updatedTask.Id == existingTask.Id))
                .ToList();

            _context.TrainingTasks.RemoveRange(tasksToRemove);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok("Training updated successfully!");
        }




        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public IActionResult Delete()
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            return View();
        }


        [HttpDelete]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.Trainings.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound("Training not found.");
            }

            _context.TrainingTasks.RemoveRange(training.Tasks);

            _context.Trainings.Remove(training);

            await _context.SaveChangesAsync();

            return Ok("Training deleted successfully.");
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Coach,Athlete")]
        public async Task<IActionResult> GetTrainingDetails(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = training.Id,
                date = training.Date.ToString("yyyy-MM-ddTHH:mm"), 
                tasks = training.Tasks.Select(t => new
                {
                    trainingSection = t.TrainingSection,
                    taskDescription = t.TaskDescription,
                    distance = t.Distance,
                    taskType = t.TaskType
                })
            });
        }

        private bool TrainingExists(int id)
        {
            return _context.Trainings.Any(e => e.Id == id);
        }
    }
}
