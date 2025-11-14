using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Models;
using SkillBridge_dotnet.Api.Models.Joins;
using SkillBridge_dotnet.Api.dtos;

namespace SkillBridge_dotnet.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/profiles")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProfileDto dto)
        {
            var profile = new Profile
            {
                FullName = dto.FullName,
                Bio = dto.Bio,
                Location = dto.Location,
                UserId = dto.UserId
            };

            _db.Profiles.Add(profile);
            await _db.SaveChangesAsync();

            return Ok(profile);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Profiles
                .Include(p => p.ProfileCompetencies)
                .ThenInclude(pc => pc.Competency)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var profile = await _db.Profiles
                .Include(p => p.ProfileCompetencies)
                .ThenInclude(pc => pc.Competency)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null) return NotFound();

            return Ok(profile);
        }

        [HttpPost("{id}/competencies")]
        public async Task<IActionResult> AddCompetency(Guid id, [FromBody] AddCompetencyToProfileDto dto)
        {
            var profile = await _db.Profiles.FindAsync(id);
            if (profile == null) return NotFound();

            var competency = await _db.Competencies.FindAsync(dto.CompetencyId);
            if (competency == null) return BadRequest("Competency not found.");

            var join = new ProfileCompetency
            {
                ProfileId = id,
                CompetencyId = dto.CompetencyId,
                SelfAssessedLevel = dto.SelfAssessedLevel
            };

            _db.ProfileCompetencies.Add(join);
            await _db.SaveChangesAsync();

            return Ok(join);
        }
    }
}
