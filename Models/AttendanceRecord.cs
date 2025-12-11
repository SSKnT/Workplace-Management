using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        [Required]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        public string? Remarks { get; set; }

        public string? MarkedById { get; set; }
        public ApplicationUser? MarkedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }
}
