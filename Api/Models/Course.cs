using System.ComponentModel.DataAnnotations;
using SkillBridge_dotnet.Api.Models.Joins;

namespace SkillBridge_dotnet.Api.Models
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        /// <summary>Estimated duration in hours.</summary>
        public int DurationHours { get; set; }

        public decimal Price { get; set; } = 0m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Competencies that the course covers
        public ICollection<CourseCompetency> CourseCompetencies { get; set; } = new List<CourseCompetency>();

        // Enrollments
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
