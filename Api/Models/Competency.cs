using SkillBridge_dotnet.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;
using SkillBridge_dotnet.Api.Models.Joins;

namespace SkillBridge_dotnet.Api.Models
{
    public class Competency
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public CompetencyLevel RecommendedLevel { get; set; } = CompetencyLevel.Beginner;

        // Relations
        public ICollection<ProfileCompetency> ProfileCompetencies { get; set; } = new List<ProfileCompetency>();
        public ICollection<CourseCompetency> CourseCompetencies { get; set; } = new List<CourseCompetency>();
        public ICollection<VacancyCompetency> VacancyCompetencies { get; set; } = new List<VacancyCompetency>();
    }
}
