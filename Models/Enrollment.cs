using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
