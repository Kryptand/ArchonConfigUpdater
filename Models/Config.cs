namespace ArchonConfigUpdater.Models;

public sealed class Config
{
    public string BaseUrl { get; set; }
    public string OutputPath { get; set; }
    public List<CharacterDetail> Characters { get; set; }
    public List<string> RaidBosses { get; set; }
    public List<string> Dungeons { get; set; }
    public List<string> RaidDifficulties { get; set; }
    public string MythicPlusTalentTimeSpan { get; set; }
}