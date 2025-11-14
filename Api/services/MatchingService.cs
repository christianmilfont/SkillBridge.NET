using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.Models.Joins;
namespace SkillBridge_dotnet.Api.Services;

public class MatchingService : IMatchingService
{
    private readonly AppDbContext _context;

    public MatchingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Profile>> MatchProfilesForCourseAsync(Guid courseId)
    {
        // Carrega competências requeridas
        var required = await _context.CourseCompetencies
            .Where(cc => cc.CourseId == courseId)
            .Select(cc => new { cc.CompetencyId, cc.RequiredLevel })
            .ToListAsync();

        if (!required.Any())
            return new List<Profile>();

        var requiredCount = required.Count;

        // Perfis que possuem TODAS as competências exigidas com nível >= requerido
        var matchingProfileIds =
            await (from pc in _context.ProfileCompetencies
                   join cc in _context.CourseCompetencies
                        on pc.CompetencyId equals cc.CompetencyId
                   where cc.CourseId == courseId
                   where pc.SelfAssessedLevel >= cc.RequiredLevel
                   select new { pc.ProfileId, pc.CompetencyId })
            .GroupBy(x => x.ProfileId)
            .Where(g => g.Count() == requiredCount)
            .Select(g => g.Key)
            .ToListAsync();

        return await _context.Profiles
            .Where(p => matchingProfileIds.Contains(p.Id))
            .Include(p => p.ProfileCompetencies)
            .ToListAsync();
    }

    public async Task<List<Profile>> MatchProfilesForVacancyAsync(Guid vacancyId)
    {
        var required = await _context.VacancyCompetencies
            .Where(vc => vc.VacancyId == vacancyId)
            .Select(vc => new { vc.CompetencyId, vc.RequiredLevel })
            .ToListAsync();

        if (!required.Any())
            return new List<Profile>();

        var requiredCount = required.Count;

        var matchingProfileIds =
            await (from pc in _context.ProfileCompetencies
                   join vc in _context.VacancyCompetencies
                        on pc.CompetencyId equals vc.CompetencyId
                   where vc.VacancyId == vacancyId
                   where pc.SelfAssessedLevel >= vc.RequiredLevel
                   select new { pc.ProfileId, pc.CompetencyId })
            .GroupBy(x => x.ProfileId)
            .Where(g => g.Count() == requiredCount)
            .Select(g => g.Key)
            .ToListAsync();

        return await _context.Profiles
            .Where(p => matchingProfileIds.Contains(p.Id))
            .Include(p => p.ProfileCompetencies)
            .ToListAsync();
    }
}
