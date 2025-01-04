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

        public async Task<IActionResult> Index()
        {
            return View(await _context.Trainings.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var training = await _context.Trainings
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
                Tasks = new List<TrainingTask> { new TrainingTask() }
            };
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Training training)
        {
            if (ModelState.IsValid)
            {
                _context.Add(training);

                foreach (var task in training.Tasks)
                {
                    task.TrainingId = training.Id; 
                    _context.Add(task);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(training);
        }


        public async Task<IActionResult> Edit()
        {
            var trainings = await _context.Trainings.ToListAsync();
            ViewBag.TrainingList = new SelectList(trainings, "Id", "Date");
            return View();
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date")] Training training)
        {
            if (id != training.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(training);
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var training = await _context.Trainings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (training == null)
            {
                return NotFound();
            }

            return View(training);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training != null)
            {
                _context.Trainings.Remove(training);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainingExists(int id)
        {
            return _context.Trainings.Any(e => e.Id == id);
        }
    }
}
