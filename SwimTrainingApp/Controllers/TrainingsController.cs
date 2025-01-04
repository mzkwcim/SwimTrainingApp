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
            return View(await _context.Trainings.ToListAsync());
        }

        // GET: Trainings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var training = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (training == null)
            {
                return NotFound();
            }

            return View(training);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Training training)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Zapisz trening, aby wygenerować jego Id
                    _context.Add(training);
                    await _context.SaveChangesAsync();

                    // Powiąż każde zadanie z `TrainingId` i zapisz je
                    foreach (var task in training.Tasks)
                    {
                        task.TrainingId = training.Id; // Ustawienie klucza obcego
                        _context.TrainingTasks.Add(task); // Dodanie zadania do kontekstu
                    }

                    await _context.SaveChangesAsync(); // Zapisz zadania w bazie danych

                    return Json(new { success = true, message = "Training and tasks created successfully!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            // Obsługa błędów walidacji
            return Json(new { success = false, message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
        }






        // GET: Trainings/Edit
        public async Task<IActionResult> Edit()
        {
            var trainings = await _context.Trainings.ToListAsync();
            ViewBag.TrainingList = new SelectList(trainings, "Id", "Date");
            return View();
        }

        // POST: Trainings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int selectedTrainingId)
        {
            if (selectedTrainingId == 0)
            {
                return NotFound();
            }

            var training = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == selectedTrainingId);

            if (training == null)
            {
                return NotFound();
            }

            return View("EditForm", training); // Otwiera widok formularza edycji
        }

        // POST: Trainings/EditForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForm(int id, [Bind("Id,Date,Tasks")] Training training)
        {
            if (id != training.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Aktualizacja treningu
                    _context.Update(training);

                    // Aktualizacja zadań
                    foreach (var task in training.Tasks)
                    {
                        if (task.Id == 0)
                        {
                            // Nowe zadanie
                            task.TrainingId = training.Id;
                            _context.Add(task);
                        }
                        else
                        {
                            // Istniejące zadanie
                            _context.Update(task);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainingExists(training.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(training);
        }

        // GET: Trainings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var training = await _context.Trainings
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (training == null)
            {
                return NotFound();
            }

            return View(training);
        }

        // POST: Trainings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var training = await _context.Trainings.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id);

            if (training != null)
            {
                // Usunięcie powiązanych zadań
                foreach (var task in training.Tasks)
                {
                    _context.TrainingTasks.Remove(task);
                }

                // Usunięcie treningu
                _context.Trainings.Remove(training);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
