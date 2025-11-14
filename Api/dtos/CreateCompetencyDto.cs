using SkillBridge_dotnet.Api.Models.Enums;

namespace SkillBridge_dotnet.Api.dtos
{ 
    public class CreateCompetencyDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public CompetencyLevel RecommendedLevel { get; set; }
    }
}