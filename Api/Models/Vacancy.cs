using SkillBridge_dotnet.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;
using SkillBridge_dotnet.Api.Models.Joins;

namespace SkillBridge_dotnet.Api.Models
{
    public class Vacancy
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string Company { get; set; }

        [MaxLength(150)]
        public string Location { get; set; }

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }

        public VacancyStatus Status { get; set; } = VacancyStatus.Draft;

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosesAt { get; set; }

        public ICollection<VacancyCompetency> VacancyCompetencies { get; set; } = new List<VacancyCompetency>();
    }
}
