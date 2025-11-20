using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.Models.Joins;

using System.Globalization;

namespace SkillBridge_dotnet.Api.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _context;

        public RecommendationService(AppDbContext context)
        {
            _context = context;
        }

        // ================================
        // 🔹 Recomendação de curso
        // ================================
        public async Task RecommendCourseAsync(Course course)
        {
            var courseCompetencies = await _context.CourseCompetencies
                .Where(cc => cc.CourseId == course.Id)
                .Include(cc => cc.Competency)
                .ToListAsync();

            // SE O CURSO TEM COMPETÊNCIAS → lógica normal
            if (courseCompetencies.Count > 0)
            {
                await RecommendByCompetency(course, courseCompetencies);
                return;
            }

            // SE NÃO TEM COMPETÊNCIAS → MACHINE LEARNING FRACO
            var keywords = ExtractKeywords(course.Title);

            var competencies = await _context.Competencies.ToListAsync();

            // 1. Encontrar competências semelhantes ao título
            var matchedCompetencies = competencies
                .Where(c => keywords.Any(k =>
                    Similarity(k, c.Name) >= 0.6 || // 60% de semelhança
                    c.Name.ToLower().Contains(k) ||
                    k.Contains(c.Name.ToLower())
                ))
                .ToList();

            // 2. Perfis que possuem essas competências
            var profiles = await _context.ProfileCompetencies
                .Where(pc => matchedCompetencies.Select(mc => mc.Id).Contains(pc.CompetencyId))
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

        // ================================
        // 🔹 Método auxiliar: recomendação por competência e nível
        // ================================
        private async Task RecommendByCompetency(Course course, List<CourseCompetency> courseCompetencies)
        {
            var neededCompetencies = courseCompetencies
                .Select(cc => new
                {
                    cc.CompetencyId,
                    RecommendedLevel = cc.Competency.RecommendedLevel
                })
                .ToList();

            var profiles = await _context.Profiles
                .Include(p => p.ProfileCompetencies)
                .ToListAsync();

            foreach (var profile in profiles)
            {
                bool isCompatible = false;

                foreach (var need in neededCompetencies)
                {
                    var pc = profile.ProfileCompetencies
                        .FirstOrDefault(x => x.CompetencyId == need.CompetencyId);

                    if (pc != null)
                    {
                        // Nível do perfil >= nível recomendado
                        if ((int)pc.SelfAssessedLevel >= (int)need.RecommendedLevel)
                        {
                            isCompatible = true;
                            break;
                        }
                    }
                }

                if (isCompatible)
                {
                    _context.Recommendations.Add(new Recommendation
                    {
                        ProfileId = profile.Id,
                        CourseId = course.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

// ================================
// 🔹 Recomendação de vagas (igual cursos)
// ================================
public async Task RecommendVacancyAsync(Vacancy vacancy)
{
    var vacancyCompetencies = await _context.VacancyCompetencies
        .Where(vc => vc.VacancyId == vacancy.Id)
        .Include(vc => vc.Competency)
        .ToListAsync();

    // 1️⃣ Se a vaga tem competências → lógica forte
    if (vacancyCompetencies.Count > 0)
    {
        await RecommendVacancyByCompetency(vacancy, vacancyCompetencies);
        return;
    }

    // 2️⃣ Se NÃO tem competências → MACHINE LEARNING FRACO
    var keywords = ExtractKeywords(vacancy.Title + " " + vacancy.Description);

    var competencies = await _context.Competencies.ToListAsync();

    // Procurar competências parecidas pelo título/descrição
    var matchedCompetencies = competencies
        .Where(c => keywords.Any(k =>
            Similarity(k, c.Name) >= 0.6 ||
            c.Name.ToLower().Contains(k) ||
            k.Contains(c.Name.ToLower())
        ))
        .ToList();

    // Perfis que têm essas skills
    var profiles = await _context.ProfileCompetencies
        .Where(pc => matchedCompetencies.Select(mc => mc.Id).Contains(pc.CompetencyId))
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

// ================================
// 🔹 Método auxiliar: recomendação de vaga por competência e nível
// ================================
private async Task RecommendVacancyByCompetency(Vacancy vacancy, List<VacancyCompetency> vacancyCompetencies)
{
    var neededCompetencies = vacancyCompetencies
        .Select(vc => new
        {
            vc.CompetencyId,
            RequiredLevel = vc.Competency.RecommendedLevel // usa mesmo campo
        })
        .ToList();

    var profiles = await _context.Profiles
        .Include(p => p.ProfileCompetencies)
        .ToListAsync();

    foreach (var profile in profiles)
    {
        bool isCompatible = false;

        foreach (var need in neededCompetencies)
        {
            var pc = profile.ProfileCompetencies
                .FirstOrDefault(x => x.CompetencyId == need.CompetencyId);

            if (pc != null)
            {
                // Nível do perfil >= nível requerido pela vaga
                if ((int)pc.SelfAssessedLevel >= (int)need.RequiredLevel)
                {
                    isCompatible = true;
                    break;
                }
            }
        }

        if (isCompatible)
        {
            _context.Recommendations.Add(new Recommendation
            {
                ProfileId = profile.Id,
                VacancyId = vacancy.Id
            });
        }
    }

    await _context.SaveChangesAsync();
}

        // ================================
        // 🔹 Helpers de Machine Learning fraco
        // ================================
        private List<string> ExtractKeywords(string text)
        {
            string[] stopWords = { "curso", "de", "do", "da", "para", "com", "bootcamp" };

            var words = text
                .ToLower()
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !stopWords.Contains(w))
                .Select(RemoveAccents)
                .ToList();

            return words;
        }

        private string RemoveAccents(string input)
        {
            return new string(input
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray())
                .ToLower();
        }

        private double Similarity(string a, string b)
        {
            a = RemoveAccents(a.ToLower());
            b = RemoveAccents(b.ToLower());

            int maxLen = Math.Max(a.Length, b.Length);
            if (maxLen == 0) return 1;

            return 1.0 - (double)ComputeLevenshteinDistance(a, b) / maxLen;
        }

        private int ComputeLevenshteinDistance(string s, string t)
        {
            int[,] dp = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                dp[i, 0] = i;
            for (int j = 0; j <= t.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost
                    );
                }
            }

            return dp[s.Length, t.Length];
        }
    }
}
