using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.dtos;

namespace SkillBridge_dotnet.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/competencies")]
    public class CompetencyController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CompetencyController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompetencyDto dto)
        {
            // Verificação para garantir que o nível está dentro dos limites
            if (!Enum.IsDefined(typeof(CompetencyLevel), dto.RecommendedLevel))
            {
                return BadRequest("O nível recomendado é inválido.");
            }

            var competency = new Competency
            {
                Name = dto.Name,
                Description = dto.Description,
                RecommendedLevel = dto.RecommendedLevel
            };

            _db.Competencies.Add(competency);
            await _db.SaveChangesAsync();

            return Ok(competency);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Competencies.ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _db.Competencies.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _db.Competencies.FindAsync(id);
            if (item == null) return NotFound();

            _db.Competencies.Remove(item);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
