using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.Services;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PdfExportService _pdfService;

        public AttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, PdfExportService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
        }

        // GET: Attendance
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var courses = User.IsInRole("Admin")
                ? await _context.Courses.Include(c => c.Teacher).Include(c => c.Enrollments).ToListAsync()
                : await _context.Courses.Include(c => c.Enrollments).Where(c => c.TeacherId == userId).ToListAsync();

            return View(courses);
        }

        // GET: Attendance/Mark/5
        public async Task<IActionResult> Mark(int? id, DateTime? date)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // Teachers can only mark attendance for their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            var selectedDate = (date ?? DateTime.Today).ToUniversalTime();

            // Get attendance records for the selected date
            var existingAttendance = await _context.AttendanceRecords
                .Where(a => a.CourseId == id && a.Date.Date == selectedDate.Date)
                .ToListAsync();

            var viewModel = new MarkAttendanceViewModel
            {
                Course = course,
                SelectedDate = selectedDate,
                StudentAttendance = course.Enrollments.Select(e => new StudentAttendanceViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    StudentEmail = e.Student.Email ?? string.Empty,
                    Status = existingAttendance.FirstOrDefault(a => a.StudentId == e.StudentId)?.Status ?? AttendanceStatus.Present,
                    Remarks = existingAttendance.FirstOrDefault(a => a.StudentId == e.StudentId)?.Remarks ?? string.Empty,
                    AttendanceRecordId = existingAttendance.FirstOrDefault(a => a.StudentId == e.StudentId)?.Id
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Attendance/Mark
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mark(MarkAttendanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload course data
                var courseData = await _context.Courses
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == model.CourseId);
                
                if (courseData != null)
                    model.Course = courseData;
                    
                return View(model);
            }

            var course = await _context.Courses.FindAsync(model.CourseId);
            if (course == null) return NotFound();

            // Teachers can only mark attendance for their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            var currentUserId = _userManager.GetUserId(User);

            foreach (var studentAttendance in model.StudentAttendance)
            {
                if (studentAttendance.AttendanceRecordId.HasValue)
                {
                    // Update existing record
                    var existingRecord = await _context.AttendanceRecords
                        .FindAsync(studentAttendance.AttendanceRecordId.Value);
                    
                    if (existingRecord != null)
                    {
                        existingRecord.Status = studentAttendance.Status;
                        existingRecord.Remarks = studentAttendance.Remarks;
                        existingRecord.MarkedById = currentUserId;
                    }
                }
                else
                {
                    // Create new record
                    var newRecord = new AttendanceRecord
                    {
                        CourseId = model.CourseId,
                        StudentId = studentAttendance.StudentId,
                        Date = model.SelectedDate.ToUniversalTime().Date,
                        Status = studentAttendance.Status,
                        Remarks = studentAttendance.Remarks,
                        MarkedById = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AttendanceRecords.Add(newRecord);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Attendance marked successfully!";
            
            return RedirectToAction(nameof(Mark), new { id = model.CourseId, date = model.SelectedDate });
        }

        // POST: Attendance/MarkAttendanceAjax (AJAX API)
        [HttpPost]
        public async Task<IActionResult> MarkAttendanceAjax([FromBody] StudentAttendanceUpdateModel model)
        {
            try
            {
                var course = await _context.Courses.FindAsync(model.CourseId);
                if (course == null)
                    return Json(new { success = false, message = "Course not found" });

                // Teachers can only mark attendance for their own courses
                if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                    return Json(new { success = false, message = "Unauthorized" });

                var currentUserId = _userManager.GetUserId(User);
                var selectedDate = model.Date.ToUniversalTime().Date;

                var existingRecord = await _context.AttendanceRecords
                    .FirstOrDefaultAsync(a => a.CourseId == model.CourseId 
                        && a.StudentId == model.StudentId 
                        && a.Date.Date == selectedDate);

                if (existingRecord != null)
                {
                    existingRecord.Status = model.Status;
                    existingRecord.Remarks = model.Remarks ?? string.Empty;
                    existingRecord.MarkedById = currentUserId;
                }
                else
                {
                    var newRecord = new AttendanceRecord
                    {
                        CourseId = model.CourseId,
                        StudentId = model.StudentId,
                        Date = selectedDate,
                        Status = model.Status,
                        Remarks = model.Remarks ?? string.Empty,
                        MarkedById = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AttendanceRecords.Add(newRecord);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Attendance updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Attendance/MarkAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAll(int courseId, DateTime date, AttendanceStatus status)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            // Teachers can only mark attendance for their own courses
            if (User.IsInRole("Teacher") && course.TeacherId != _userManager.GetUserId(User))
                return Forbid();

            var currentUserId = _userManager.GetUserId(User);
            var selectedDate = date.ToUniversalTime().Date;

            // Get existing attendance records for the date
            var existingRecords = await _context.AttendanceRecords
                .Where(a => a.CourseId == courseId && a.Date.Date == selectedDate)
                .ToListAsync();

            foreach (var enrollment in course.Enrollments)
            {
                var existingRecord = existingRecords.FirstOrDefault(a => a.StudentId == enrollment.StudentId);
                
                if (existingRecord != null)
                {
                    existingRecord.Status = status;
                    existingRecord.MarkedById = currentUserId;
                }
                else
                {
                    var newRecord = new AttendanceRecord
                    {
                        CourseId = courseId,
                        StudentId = enrollment.StudentId,
                        Date = selectedDate,
                        Status = status,
                        MarkedById = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.AttendanceRecords.Add(newRecord);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"All students marked as {status}!";
            
            return RedirectToAction(nameof(Mark), new { id = courseId, date = date });
        }

        // GET: Attendance/ExportDaily/5?date=2024-12-06
        public async Task<IActionResult> ExportDaily(int id, DateTime? date)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Check authorization
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && course.TeacherId != userId)
            {
                return Forbid();
            }

            var selectedDate = (date ?? DateTime.Today).ToUniversalTime().Date;
            
            var records = await _context.AttendanceRecords
                .Include(a => a.Student)
                .Where(a => a.CourseId == id && a.Date.Date == selectedDate)
                .OrderBy(a => a.Student.FullName)
                .ToListAsync();

            if (!records.Any())
            {
                TempData["Error"] = "No attendance records found for this date.";
                return RedirectToAction(nameof(Mark), new { id, date });
            }

            var pdfBytes = _pdfService.GenerateAttendanceReport(course, selectedDate, records);
            var fileName = $"{course.CourseCode}_Attendance_{selectedDate:yyyy-MM-dd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Attendance/ExportCourseSummary/5
        public async Task<IActionResult> ExportCourseSummary(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Check authorization
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && course.TeacherId != userId)
            {
                return Forbid();
            }

            var students = course.Enrollments
                .Where(e => e.IsActive)
                .Select(e => e.Student)
                .OrderBy(s => s.FullName)
                .ToList();

            if (!students.Any())
            {
                TempData["Error"] = "No students enrolled in this course.";
                return RedirectToAction("Details", "Course", new { id });
            }

            // Get all attendance records for this course
            var allRecords = await _context.AttendanceRecords
                .Include(a => a.Student)
                .Where(a => a.CourseId == id)
                .ToListAsync();

            // Group by student
            var attendanceByStudent = allRecords
                .GroupBy(a => a.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var pdfBytes = _pdfService.GenerateCourseAttendanceSummary(course, students, attendanceByStudent);
            var fileName = $"{course.CourseCode}_Summary_{DateTime.Now:yyyy-MM-dd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
    }

    // Model for AJAX attendance update
    public class StudentAttendanceUpdateModel
    {
        public int CourseId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }
}
