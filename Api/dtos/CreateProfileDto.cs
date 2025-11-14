namespace SkillBridge_dotnet.Api.dtos
{ 
    public class CreateProfileDto
    {
        public string FullName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public Guid? UserId { get; set; }
    }
}