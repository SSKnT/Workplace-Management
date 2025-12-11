using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Enrollment/Manage/5
        public async Task<IActionResult> Manage(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // Teachers can only manage their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            // Get all students
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var enrolledStudentIds = course.Enrollments.Select(e => e.StudentId).ToHashSet();
            
            ViewBag.Course = course;
            ViewBag.AvailableStudents = students.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();
            
            return View(course);
        }

        // POST: Enrollment/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId, string studentId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            // Teachers can only enroll in their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (existingEnrollment != null)
            {
                TempData["Error"] = "Student is already enrolled in this course.";
                return RedirectToAction(nameof(Manage), new { id = courseId });
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Student enrolled successfully!";
            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        // POST: Enrollment/BulkEnroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkEnroll(int courseId, List<string> studentIds)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            // Teachers can only enroll in their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            if (studentIds == null || !studentIds.Any())
            {
                TempData["Error"] = "No students selected.";
                return RedirectToAction(nameof(Manage), new { id = courseId });
            }

            var existingEnrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId && studentIds.Contains(e.StudentId))
                .Select(e => e.StudentId)
                .ToListAsync();

            var newStudentIds = studentIds.Except(existingEnrollments).ToList();
            
            foreach (var studentId in newStudentIds)
            {
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    EnrolledAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Enrollments.Add(enrollment);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{newStudentIds.Count} student(s) enrolled successfully!";
            if (existingEnrollments.Any())
            {
                TempData["Warning"] = $"{existingEnrollments.Count} student(s) were already enrolled.";
            }

            return RedirectToAction(nameof(Manage), new { id = courseId });
        }

        // POST: Enrollment/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null) return NotFound();

            // Teachers can only remove from their own courses
            if (User.IsInRole("Teacher") && enrollment.Course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Student removed from course successfully!";
            return RedirectToAction(nameof(Manage), new { id = enrollment.CourseId });
        }
    }
}
