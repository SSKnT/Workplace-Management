using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    public class StudentCourseViewModel
    {
        public Course Course { get; set; } = null!;
        public DateTime EnrolledDate { get; set; }
        public int TotalClasses { get; set; }
        public int AttendedClasses { get; set; }
        public double AttendancePercentage { get; set; }
    }

    public class StudentAttendanceDetailViewModel
    {
        public Course Course { get; set; } = null!;
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        
        public double AttendancePercentage => TotalClasses > 0 
            ? (double)(PresentCount + LateCount) / TotalClasses * 100 
            : 0;
    }
}
