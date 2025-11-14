using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.Models.Enums;
using SkillBridge_dotnet.Api.Models.Joins;
using SkillBridge_dotnet.Api.Services;

namespace SkillBridge_dotnet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacancyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RecommendationService _recom;

        public VacancyController(AppDbContext context)
        {
            _context = context;
            _recom = new RecommendationService(context);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVacancy([FromBody] CreateVacancyDto dto)
        {
            var vacancy = new Vacancy
            {
                Title = dto.Title,
                Description = dto.Description,
                Company = dto.Company, // ← obrigatório
                Location = dto.Location
                Status = VacancyStatus.Open
            };

            _context.Vacancies.Add(vacancy);
            await _context.SaveChangesAsync();

            foreach (var comp in dto.Competencies)
            {
                _context.VacancyCompetencies.Add(new VacancyCompetency
                {
                    VacancyId = vacancy.Id,
                    CompetencyId = comp.CompetencyId,
                    RequiredLevel = comp.RequiredLevel
                });
            }

            await _context.SaveChangesAsync();

            // 🔹 Recomendação automática
            await _recom.RecommendVacancyAsync(vacancy);

            return Ok(vacancy);
        }

        public class CreateVacancyDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Company { get; set; }   // OBRIGATÓRIO
            public string Location { get; set; }   // OBRIGATÓRIO

            public List<CompetencyInput> Competencies { get; set; }
        }

        public class CompetencyInput
        {
            public Guid CompetencyId { get; set; }
            public CompetencyLevel RequiredLevel { get; set; }
        }
    }
}
