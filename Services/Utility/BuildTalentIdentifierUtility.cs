namespace ArchonConfigUpdater.Services.Utility;

public static class BuildTalentIdentifierUtility
{
    public static string BuildTalentIdentifier(string ContentType, string Difficulty, string Encounter)
    {
        return $"{ContentType}-{Difficulty}-{Encounter}";
    }
}