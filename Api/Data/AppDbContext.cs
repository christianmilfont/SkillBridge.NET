using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.Models.Joins;
using System;
using System.Linq;

namespace SkillBridge_dotnet.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Main entities
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Competency> Competencies { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        // Join entities
        public DbSet<ProfileCompetency> ProfileCompetencies { get; set; }
        public DbSet<CourseCompetency> CourseCompetencies { get; set; }
        public DbSet<VacancyCompetency> VacancyCompetencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // 🔹 ENUM CONFIGURATIONS
            // ======================

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Enrollment>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Vacancy>()
                .Property(v => v.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Competency>()
                .Property(c => c.RecommendedLevel)
                .HasConversion<string>();

            modelBuilder.Entity<VacancyCompetency>()
                .Property(vc => vc.RequiredLevel)
                .HasConversion<string>();

            modelBuilder.Entity<ProfileCompetency>()
                .Property(pc => pc.SelfAssessedLevel)
                .HasConversion<string>();

            // ==========================
            // 🔹 RELATIONSHIPS & JOINS
            // ==========================

            // User <-> Profile (1:1 optional)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Profile <-> Competency (M:N via join)
            modelBuilder.Entity<ProfileCompetency>()
                .HasKey(pc => new { pc.ProfileId, pc.CompetencyId });

            modelBuilder.Entity<ProfileCompetency>()
                .HasOne(pc => pc.Profile)
                .WithMany(p => p.ProfileCompetencies)
                .HasForeignKey(pc => pc.ProfileId);

            modelBuilder.Entity<ProfileCompetency>()
                .HasOne(pc => pc.Competency)
                .WithMany(c => c.ProfileCompetencies)
                .HasForeignKey(pc => pc.CompetencyId);

            // Course <-> Competency (M:N)
            modelBuilder.Entity<CourseCompetency>()
                .HasKey(cc => new { cc.CourseId, cc.CompetencyId });

            modelBuilder.Entity<CourseCompetency>()
                .HasOne(cc => cc.Course)
                .WithMany(c => c.CourseCompetencies)
                .HasForeignKey(cc => cc.CourseId);

            modelBuilder.Entity<CourseCompetency>()
                .HasOne(cc => cc.Competency)
                .WithMany(c => c.CourseCompetencies)
                .HasForeignKey(cc => cc.CompetencyId);

            // Vacancy <-> Competency (M:N)
            modelBuilder.Entity<VacancyCompetency>()
                .HasKey(vc => new { vc.VacancyId, vc.CompetencyId });

            modelBuilder.Entity<VacancyCompetency>()
                .HasOne(vc => vc.Vacancy)
                .WithMany(v => v.VacancyCompetencies)
                .HasForeignKey(vc => vc.VacancyId);

            modelBuilder.Entity<VacancyCompetency>()
                .HasOne(vc => vc.Competency)
                .WithMany(c => c.VacancyCompetencies)
                .HasForeignKey(vc => vc.CompetencyId);

            // Enrollment (User <-> Course)
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional unique constraint for single enrollment per course/user
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique(false);

            // ==========================
            // 🔹 MYSQL-SPECIFIC SETTINGS
            // ==========================

            // Default charset for MySQL tables
            modelBuilder.UseCollation("utf8mb4_general_ci");
            modelBuilder.HasCharSet("utf8mb4");

            // Set decimal precision globally (e.g. DECIMAL(10,2))
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(10);
                property.SetScale(2);
            }

            // Configure DateTime to use UTC
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(DateTime)))
            {
                property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                ));
            }

            // Optional: table naming conventions (snake_case)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));
            }
        }

        // Helper: Converts PascalCase -> snake_case (for MySQL consistency)
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var chars = input.ToCharArray();
            var result = "";

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsUpper(chars[i]))
                {
                    if (i > 0)
                        result += "_";
                    result += char.ToLower(chars[i]);
                }
                else
                {
                    result += chars[i];
                }
            }

            return result;
        }
    }
}
