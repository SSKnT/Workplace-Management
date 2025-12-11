using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;

namespace AttendanceSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
            {
                // Admin statistics
                ViewBag.TotalCourses = await _context.Courses.CountAsync();
                
                var teacherRole = await _userManager.GetUsersInRoleAsync("Teacher");
                ViewBag.TotalTeachers = teacherRole.Count;
                
                var studentRole = await _userManager.GetUsersInRoleAsync("Student");
                ViewBag.TotalStudents = studentRole.Count;
                
                ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
            }
            else if (User.IsInRole("Teacher"))
            {
                var userId = _userManager.GetUserId(User);
                
                // Teacher statistics
                ViewBag.MyCourses = await _context.Courses
                    .Where(c => c.TeacherId == userId)
                    .CountAsync();
                
                ViewBag.TotalStudents = await _context.Enrollments
                    .Where(e => e.Course.TeacherId == userId && e.IsActive)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .CountAsync();
                
                ViewBag.TodaysClasses = await _context.Courses
                    .Where(c => c.TeacherId == userId)
                    .CountAsync();
            }
            else if (User.IsInRole("Student"))
            {
                var userId = _userManager.GetUserId(User);
                
                // Student statistics
                ViewBag.EnrolledCourses = await _context.Enrollments
                    .Where(e => e.StudentId == userId && e.IsActive)
                    .CountAsync();
                
                ViewBag.TotalAttendance = await _context.AttendanceRecords
                    .Where(a => a.StudentId == userId)
                    .CountAsync();
                
                var attendanceStats = await _context.AttendanceRecords
                    .Where(a => a.StudentId == userId)
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();
                
                var presentCount = attendanceStats.FirstOrDefault(s => s.Status == AttendanceStatus.Present)?.Count ?? 0;
                var lateCount = attendanceStats.FirstOrDefault(s => s.Status == AttendanceStatus.Late)?.Count ?? 0;
                var totalRecords = attendanceStats.Sum(s => s.Count);
                
                ViewBag.AttendanceRate = totalRecords > 0 
                    ? Math.Round((double)(presentCount + lateCount) / totalRecords * 100, 1)
                    : 0;
            }
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
