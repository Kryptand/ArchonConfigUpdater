using System.Text.Json;
using ArchonConfigUpdater.Models;

public class ParseSettings
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
            throw new Exception("Failed to parse settings file");
        }

        return config;
    }
}