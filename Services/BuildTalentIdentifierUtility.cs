namespace ArchonConfigUpdater.Services;

internal class BuildTalentIdentifierUtility
{
    public static string BuildTalentIdentifier(string ContentType, string Difficulty, string Encounter)
    {
        return $"{ContentType}-{Difficulty}-{Encounter}";
    }
}