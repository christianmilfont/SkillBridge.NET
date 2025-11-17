using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.Models.Joins;
using SkillBridge_dotnet.Api.Services;

namespace SkillBridge_dotnet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RecommendationService _recom;

        public CourseController(AppDbContext context)
        {
            _context = context;
            _recom = new RecommendationService(context);
        }
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourse(Guid courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCompetencies)
                .ThenInclude(cc => cc.Competency) // Inclui as competências associadas
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound(new { message = "Curso não encontrado." });
            }

            return Ok(new
            {
                course.Id,
                course.Title,
                course.Description,
                CourseCompetencies = course.CourseCompetencies.Select(c => new
                {
                    c.CompetencyId,
                    CompetencyName = c.Competency.Name // Nome da competência
                })
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            foreach (var compId in dto.CompetencyIds)
            {
                _context.CourseCompetencies.Add(new CourseCompetency
                {
                    CourseId = course.Id,
                    CompetencyId = compId
                });
            }

            await _context.SaveChangesAsync();

            // 🔄 Recarrega o curso com as competencies
            await _context.Entry(course)
                .Collection(c => c.CourseCompetencies)
                .LoadAsync();

            // 🔹 Recomenda automaticamente perfis
            await _recom.RecommendCourseAsync(course);

            return Ok(new
            {
                course.Id,
                course.Title,
                Competencies = course.CourseCompetencies.Select(c => new
                {
                    c.CompetencyId
                })
            });
        }

        public class CreateCourseDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public List<Guid> CompetencyIds { get; set; }
        }
    }
}
