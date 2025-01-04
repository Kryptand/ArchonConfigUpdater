namespace ArchonConfigUpdater.Services.Contracts;

public interface ITalentSource
{
    public Task<string> GetDungeonTalentSelectionAsync(string className, string spec,
        string difficulty, string encounter);

    public Task<string> GetRaidTalentSelectionAsync(string className, string spec,
        string difficulty, string encounter);
}