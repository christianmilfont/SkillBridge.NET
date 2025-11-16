using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;

namespace SkillBridge_dotnet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecommendationController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Cursos recomendados para o perfil
        [HttpGet("courses/{profileId}")]
        public async Task<IActionResult> GetRecommendedCourses(Guid profileId)
        {
            var list = await _context.Recommendations
                .Where(r => r.ProfileId == profileId && r.CourseId != null)
                .Include(r => r.Course)
                .Select(r => new
                {
                    r.Course.Id,
                    r.Course.Title,
                    r.Course.Description
                })
                .ToListAsync();

            return Ok(list);
        }

        // 🔹 Vagas recomendadas para o perfil
        [HttpGet("vacancies/{profileId}")]
        public async Task<IActionResult> GetRecommendedVacancies(Guid profileId)
        {
            var list = await _context.Recommendations
                .Where(r => r.ProfileId == profileId && r.VacancyId != null)
                .Include(r => r.Vacancy)
                .Select(r => new
                {
                    r.Vacancy.Id,
                    r.Vacancy.Title,
                    r.Vacancy.Description,
                    r.Vacancy.Company,
                    r.Vacancy.Location,
                    r.Vacancy.Status
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
