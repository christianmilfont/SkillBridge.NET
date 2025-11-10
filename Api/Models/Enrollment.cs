using SkillBridge_dotnet.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SkillBridge_dotnet.Api.Models
{
    public class Enrollment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        /// <summary>Progress 0.0 - 100.0</summary>
        public double Progress { get; set; } = 0.0;

        /// <summary>Optional score/grade</summary>
        public double? Score { get; set; }
    }
}
