using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int CreditHours { get; set; }

        public string? TeacherId { get; set; }
        public ApplicationUser? Teacher { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
