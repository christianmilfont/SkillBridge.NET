using SkillBridge_dotnet.Api.Models.Enums;

namespace SkillBridge_dotnet.Api.Models.Joins
// Join entities to represent many-to-many relations and allow metadata later:

{
    public class CourseCompetency
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public Guid CompetencyId { get; set; }
        public Competency Competency { get; set; }

        // e.g. weight or coverage percentage
        public int CoveragePercent { get; set; }
    }
}
