using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services.Utility;

public static class VerifyConfigUtility
{
    public static void VerifyConfig(Config config)
    {
        if (!config.Characters.Any())
        {
            throw new Exception("No characters found in config");
        }

        if (string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            throw new Exception("Base URL is missing from config");
        }

        if (string.IsNullOrWhiteSpace(config.OutputPath))
        {
            throw new Exception("Output path is missing from config");
        }

        if (!config.RaidBosses.Any())
        {
            throw new Exception("No raid bosses found in config");
        }

        if (!config.Dungeons.Any())
        {
            throw new Exception("No dungeons found in config");
        }

        if (!config.RaidDifficulties.Any())
        {
            throw new Exception("No raid difficulties found in config");
        }

        foreach (var character in config.Characters)
        {
            if (string.IsNullOrWhiteSpace(character.Name))
            {
                throw new Exception("Character name is missing from config");
            }

            if (character.Specializations == null || !character.Specializations.Any())
            {
                throw new Exception("No specializations found for character in config");
            }
        }
        
        // Ensure update settings exist
        if (config.Update == null)
        {
            config.Update = new UpdateSettings();
        }
    }
}