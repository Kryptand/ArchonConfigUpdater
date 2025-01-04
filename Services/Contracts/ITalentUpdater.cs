using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services.Contracts;

public interface ITalentUpdater
{
    public Task UpdateTalentsAsync(Config config, List<Talent> talents);
}