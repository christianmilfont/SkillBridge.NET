using System.ComponentModel.DataAnnotations;

namespace SkillBridge_dotnet.Api.Models
{
    public class Recommendation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; }

        public Guid? CourseId { get; set; }
        public Course Course { get; set; }

        public Guid? VacancyId { get; set; }
        public Vacancy Vacancy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
