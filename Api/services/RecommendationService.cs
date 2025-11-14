using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;

namespace SkillBridge_dotnet.Api.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _context;

        public RecommendationService(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Recomendar curso para perfis compatíveis
        public async Task RecommendCourseAsync(Course course)
        {
            var courseCompetencies = await _context.CourseCompetencies
                .Where(cc => cc.CourseId == course.Id)
                .Select(cc => cc.CompetencyId)
                .ToListAsync();

            var profiles = await _context.ProfileCompetencies
                .Where(pc => courseCompetencies.Contains(pc.CompetencyId))
                .Select(pc => pc.Profile)
                .Distinct()
                .ToListAsync();

            foreach (var profile in profiles)
            {
                _context.Recommendations.Add(new Recommendation
                {
                    ProfileId = profile.Id,
                    CourseId = course.Id
                });
            }

            await _context.SaveChangesAsync();
        }

        // 🔹 Recomendar vaga para perfis compatíveis
        public async Task RecommendVacancyAsync(Vacancy vacancy)
        {
            var vacancyCompetencies = await _context.VacancyCompetencies
                .Where(vc => vc.VacancyId == vacancy.Id)
                .Select(vc => vc.CompetencyId)
                .ToListAsync();

            var profiles = await _context.ProfileCompetencies
                .Where(pc => vacancyCompetencies.Contains(pc.CompetencyId))
                .Select(pc => pc.Profile)
                .Distinct()
                .ToListAsync();

            foreach (var profile in profiles)
            {
                _context.Recommendations.Add(new Recommendation
                {
                    ProfileId = profile.Id,
                    VacancyId = vacancy.Id
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
