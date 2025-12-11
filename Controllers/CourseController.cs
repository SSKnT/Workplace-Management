using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;

namespace AttendanceSystem.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Course
        public async Task<IActionResult> Index()
        {
            var courses = User.IsInRole("Admin")
                ? await _context.Courses
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .ToListAsync()
                : await _context.Courses
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .Where(c => c.TeacherId == _userManager.GetUserId(User))
                    .ToListAsync();

            return View(courses);
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            // Teachers can only view their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            return View(course);
        }

        // GET: Course/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateTeachersDropdown();
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.UtcNow;
                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateTeachersDropdown(course.TeacherId);
            return View(course);
        }

        // GET: Course/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            await PopulateTeachersDropdown(course.TeacherId);
            return View(course);
        }

        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing course from database (with tracking)
                    var existingCourse = await _context.Courses.FindAsync(id);
                    if (existingCourse == null) return NotFound();

                    // Update only the editable fields
                    existingCourse.Name = course.Name;
                    existingCourse.CourseCode = course.CourseCode;
                    existingCourse.Description = course.Description;
                    existingCourse.CreditHours = course.CreditHours;
                    existingCourse.TeacherId = course.TeacherId;
                    // CreatedAt is not updated - it keeps its original value

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Course updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateTeachersDropdown(course.TeacherId);
            return View(course);
        }

        // GET: Course/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        private async Task PopulateTeachersDropdown(string? selectedTeacher = null)
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", selectedTeacher);
        }
    }
}
