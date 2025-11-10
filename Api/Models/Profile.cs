using System.ComponentModel.DataAnnotations;
using SkillBridge_dotnet.Api.Models.Joins;
namespace SkillBridge_dotnet.Api.Models
{
    public class Profile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(1000)]
        public string Bio { get; set; }

        [MaxLength(150)]
        public string Location { get; set; }

        // FK to User (one-to-one)
        public Guid? UserId { get; set; }
        public User User { get; set; }

        // Competencies associated with profile (many-to-many via join)
        public ICollection<ProfileCompetency> ProfileCompetencies { get; set; } = new List<ProfileCompetency>();
    }
}
