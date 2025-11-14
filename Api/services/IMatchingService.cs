using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge_dotnet.Api.Models;
namespace SkillBridge_dotnet.Api.Services;

public interface IMatchingService
{
    Task<List<Profile>> MatchProfilesForCourseAsync(Guid courseId);
    Task<List<Profile>> MatchProfilesForVacancyAsync(Guid vacancyId);
}
