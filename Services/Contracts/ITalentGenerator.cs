using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services.Contracts;

public interface ITalentGenerator
{
    Task<List<Talent>> GenerateTalents(Config config);
}