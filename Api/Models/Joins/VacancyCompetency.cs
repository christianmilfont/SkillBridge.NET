using SkillBridge_dotnet.Api.Models.Enums;

namespace SkillBridge_dotnet.Api.Models.Joins
// Join entities to represent many-to-many relations and allow metadata later:

{
    public class VacancyCompetency
    {
        public Guid VacancyId { get; set; }
        public Vacancy Vacancy { get; set; }

        public Guid CompetencyId { get; set; }
        public Competency Competency { get; set; }

        // e.g. required level
        public CompetencyLevel RequiredLevel { get; set; } = CompetencyLevel.Beginner;
        public bool IsMandatory { get; set; } = false;
    }
}
