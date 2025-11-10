using SkillBridge_dotnet.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SkillBridge_dotnet.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // One-to-one optional Profile
        public Profile Profile { get; set; }

        // Enrollments (User <-> Course)
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
