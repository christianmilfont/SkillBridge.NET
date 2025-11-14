using SkillBridge_dotnet.Api.Models.Enums;

namespace SkillBridge_dotnet.Api.dtos
{ 
    public class AddCompetencyToProfileDto
    {
        public Guid CompetencyId { get; set; }
        public CompetencyLevel SelfAssessedLevel  { get; set; }
    }
}