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
    public class TrainingsController : Controller
    {
        private readonly AppDbContext _context;

        public TrainingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Trainings
        public async Task<IActionResult> Index()
        {
            // Pobierz wszystkie treningi z bazy danych
            var trainings = await _context.Trainings.ToListAsync();

            // Przekaż listę treningów do widoku
            return View(trainings);
        }


        // GET: Trainings/Details
        public async Task<IActionResult> Details(int? id)
        {
            // Pobierz wszystkie treningi
            var trainings = await _context.Trainings.ToListAsync();

            // Jeśli `id` jest null, wyświetl tylko listę treningów bez szczegółów
            if (id == null)
            {
                ViewBag.Trainings = trainings; // Przekazanie listy treningów do widoku
                return View();
            }

            // Pobierz szczegóły treningu na podstawie `id`
            var training = await _context.Trainings
                .Include(t => t.Tasks) // Pobierz powiązane zadania
                .FirstOrDefaultAsync(m => m.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            ViewBag.Trainings = trainings; // Przekazanie listy treningów do widoku
            return View(training); // Przekazanie szczegółowego treningu do widoku
        }



        public IActionResult Create()
        {
            var model = new Training
            {
                Tasks = new List<TrainingTask> { new TrainingTask() } // Jedno domyślne zadanie
            };

            // Wczytaj komunikaty z TempData, jeśli istnieją
            ViewBag.Message = TempData["Message"];
            ViewBag.IsSuccess = TempData["IsSuccess"];
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Training training)
        {
            // Sprawdzenie, czy obiekt Training został poprawnie przesłany
            if (training == null)
            {
                Console.WriteLine("Training object is null.");
                return BadRequest("Training object is null.");
            }

            // Logowanie daty do konsoli
            Console.WriteLine($"Received Date: {training.Date}");

            // Logowanie listy zadań (opcjonalnie)
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
                // Dodanie treningu do bazy danych
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


        // GET: Trainings/Edit
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
        public async Task<IActionResult> Edit([FromBody] Training model)
        {
            if (model == null || model.Id <= 0)
            {
                return BadRequest("Invalid training data.");
            }

            var training = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (training == null)
            {
                return NotFound("Training not found.");
            }

            // Aktualizacja daty
            training.Date = model.Date;

            // Aktualizacja zadań
            foreach (var updatedTask in model.Tasks)
            {
                var existingTask = training.Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);
                if (existingTask != null)
                {
                    existingTask.TrainingSection = updatedTask.TrainingSection;
                    existingTask.TaskDescription = updatedTask.TaskDescription;
                    existingTask.Distance = updatedTask.Distance;
                    existingTask.TaskType = updatedTask.TaskType;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Training updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving training: {ex.Message}");
                return StatusCode(500, "An error occurred while saving changes.");
            }
        }



        [HttpGet]
        public IActionResult Delete()
        {
            ViewBag.Trainings = _context.Trainings.ToList();
            return View();
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.Trainings.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound("Training not found.");
            }

            // Usuń wszystkie zadania związane z treningiem
            _context.TrainingTasks.RemoveRange(training.Tasks);

            // Usuń trening
            _context.Trainings.Remove(training);

            await _context.SaveChangesAsync();

            return Ok("Training deleted successfully.");
        }


        // API: Trainings/GetTrainingDetails/5
        [HttpGet]
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
                date = training.Date.ToString("yyyy-MM-ddTHH:mm"), // Format dla datetime-local
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
