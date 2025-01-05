using System.Text.Json;
using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services.Utility;

public class ParseSettingsUtility
{
    public Config ParseFile(string filePath)
    {
        var jsonString = File.ReadAllText(filePath);

        var config = JsonSerializer.Deserialize<Config>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null)
        {
            throw new Exception(
                "Settings file is empty or invalid. Please check if the file './settings.json' exists and is valid.");
        }

        return config;
    }
}