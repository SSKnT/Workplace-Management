using AttendanceSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels
{
    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        
        [Required]
        public DateTime SelectedDate { get; set; }
        
        public List<StudentAttendanceViewModel> StudentAttendance { get; set; } = new();
    }

    public class StudentAttendanceViewModel
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
        public int? AttendanceRecordId { get; set; }
    }
}
