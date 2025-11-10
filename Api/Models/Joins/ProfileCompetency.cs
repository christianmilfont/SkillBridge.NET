using SkillBridge_dotnet.Api.Models.Enums;

namespace SkillBridge_dotnet.Api.Models.Joins
// Join entities to represent many-to-many relations and allow metadata later:

{
    public class ProfileCompetency
    {
        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; }

        public Guid CompetencyId { get; set; }
        public Competency Competency { get; set; }

        // Example metadata: self-assessed level, years of experience
        public CompetencyLevel? SelfAssessedLevel { get; set; }
        public int? YearsExperience { get; set; }
    }
}
