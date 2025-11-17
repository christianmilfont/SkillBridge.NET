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
            var recommendedCourses = await _context.Recommendations
                .Where(r => r.ProfileId == profileId && r.CourseId != null)
                .Include(r => r.Course)
                .Select(r => new
                {
                    CourseId = r.CourseId,
                    Title = r.Course != null ? r.Course.Title : "",
                    Description = r.Course != null ? r.Course.Description : ""
                })
                .ToListAsync();

            return Ok(recommendedCourses);
        }

        // 🔹 Vagas recomendadas para o perfil
        [HttpGet("vacancies/{profileId}")]
        public async Task<IActionResult> GetRecommendedVacancies(Guid profileId)
        {
            var recommendedVacancies = await _context.Recommendations
                .Where(r => r.ProfileId == profileId && r.VacancyId != null)
                .Include(r => r.Vacancy)
                .Select(r => new
                {
                    VacancyId = r.VacancyId,
                    Title = r.Vacancy != null ? r.Vacancy.Title : "",
                    Description = r.Vacancy != null ? r.Vacancy.Description : "",
                    Company = r.Vacancy != null ? r.Vacancy.Company : "",
                    Location = r.Vacancy != null ? r.Vacancy.Location : "",
                    Status = r.Vacancy != null ? r.Vacancy.Status.ToString() : ""
                })
                .ToListAsync();

            return Ok(recommendedVacancies);
        }
    }
}
