using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.Services;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PdfExportService _pdfService;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, PdfExportService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
        }

        // GET: Student/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c.AttendanceRecords)
                .Where(e => e.StudentId == userId && e.IsActive)
                .ToListAsync();

            var courseViewModels = new List<StudentCourseViewModel>();

            foreach (var enrollment in enrollments)
            {
                var totalClasses = await _context.AttendanceRecords
                    .Where(a => a.CourseId == enrollment.CourseId)
                    .Select(a => a.Date)
                    .Distinct()
                    .CountAsync();

                var attendedClasses = await _context.AttendanceRecords
                    .Where(a => a.CourseId == enrollment.CourseId && 
                                a.StudentId == userId && 
                                (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
                    .CountAsync();

                var attendancePercentage = totalClasses > 0 
                    ? (double)attendedClasses / totalClasses * 100 
                    : 0;

                courseViewModels.Add(new StudentCourseViewModel
                {
                    Course = enrollment.Course,
                    EnrolledDate = enrollment.EnrolledAt,
                    TotalClasses = totalClasses,
                    AttendedClasses = attendedClasses,
                    AttendancePercentage = attendancePercentage
                });
            }

            return View(courseViewModels);
        }

        // GET: Student/AvailableCourses
        public async Task<IActionResult> AvailableCourses()
        {
            var userId = _userManager.GetUserId(User);
            
            // Get courses student is already enrolled in
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == userId && e.IsActive)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Get all available courses not yet enrolled in
            var availableCourses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .OrderBy(c => c.CourseCode)
                .ToListAsync();

            return View(availableCourses);
        }

        // POST: Student/EnrollInCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollInCourse(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            
            // Check if course exists
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsActive)
                {
                    TempData["Error"] = "You are already enrolled in this course.";
                }
                else
                {
                    // Reactivate enrollment
                    existingEnrollment.IsActive = true;
                    existingEnrollment.EnrolledAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Successfully re-enrolled in the course!";
                }
                return RedirectToAction(nameof(AvailableCourses));
            }

            // Create new enrollment
            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = userId,
                EnrolledAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully enrolled in {course.Name}!";
            return RedirectToAction(nameof(AvailableCourses));
        }

        // POST: Student/DropCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropCourse(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId && e.IsActive);

            if (enrollment == null)
            {
                TempData["Error"] = "Enrollment not found.";
                return RedirectToAction(nameof(MyCourses));
            }

            // Deactivate enrollment instead of deleting
            enrollment.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Successfully dropped {enrollment.Course.Name}.";
            return RedirectToAction(nameof(MyCourses));
        }

        // GET: Student/MyAttendance/5
        public async Task<IActionResult> MyAttendance(int? id)
        {
            var userId = _userManager.GetUserId(User);

            if (id == null)
            {
                // Show all attendance
                var allAttendance = await _context.AttendanceRecords
                    .Include(a => a.Course)
                    .Include(a => a.MarkedBy)
                    .Where(a => a.StudentId == userId)
                    .OrderByDescending(a => a.Date)
                    .Take(50)
                    .ToListAsync();

                return View("AllAttendance", allAttendance);
            }
            else
            {
                // Show attendance for specific course
                var enrollment = await _context.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Teacher)
                    .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == userId);

                if (enrollment == null)
                    return NotFound();

                var attendanceRecords = await _context.AttendanceRecords
                    .Include(a => a.MarkedBy)
                    .Where(a => a.CourseId == id && a.StudentId == userId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                var totalClasses = await _context.AttendanceRecords
                    .Where(a => a.CourseId == id)
                    .Select(a => a.Date)
                    .Distinct()
                    .CountAsync();

                var viewModel = new StudentAttendanceDetailViewModel
                {
                    Course = enrollment.Course,
                    AttendanceRecords = attendanceRecords,
                    TotalClasses = totalClasses,
                    PresentCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent),
                    LateCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Late),
                    ExcusedCount = attendanceRecords.Count(a => a.Status == AttendanceStatus.Excused)
                };

                return View("CourseAttendance", viewModel);
            }
        }

        // GET: Student/ExportMyAttendance
        public async Task<IActionResult> ExportMyAttendance()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Where(e => e.StudentId == userId && e.IsActive)
                .ToListAsync();

            if (!enrollments.Any())
            {
                TempData["Error"] = "You are not enrolled in any courses.";
                return RedirectToAction(nameof(MyCourses));
            }

            var courses = enrollments.Select(e => e.Course).ToList();
            
            // Get all attendance records for this student
            var allRecords = await _context.AttendanceRecords
                .Include(a => a.Course)
                .Where(a => a.StudentId == userId)
                .ToListAsync();

            // Group by course
            var attendanceByCourse = allRecords
                .GroupBy(a => a.CourseId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var pdfBytes = _pdfService.GenerateStudentAttendanceReport(user, courses, attendanceByCourse);
            var fileName = $"{user.FullName.Replace(" ", "_")}_Attendance_{DateTime.Now:yyyy-MM-dd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Student/ExportCourseAttendance/5
        public async Task<IActionResult> ExportCourseAttendance(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == userId);

            if (enrollment == null)
            {
                return NotFound();
            }

            var records = await _context.AttendanceRecords
                .Include(a => a.Course)
                .Where(a => a.CourseId == id && a.StudentId == userId)
                .ToListAsync();

            var courses = new List<Course> { enrollment.Course };
            var attendanceByCourse = new Dictionary<int, List<AttendanceRecord>>
            {
                { id, records }
            };

            var pdfBytes = _pdfService.GenerateStudentAttendanceReport(user, courses, attendanceByCourse);
            var fileName = $"{user.FullName.Replace(" ", "_")}_{enrollment.Course.CourseCode}_Attendance_{DateTime.Now:yyyy-MM-dd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
