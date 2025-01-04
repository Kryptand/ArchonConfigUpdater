using System.Text.Json.Serialization;
using ArchonConfigUpdater.Converter;

namespace ArchonConfigUpdater.Models;

public sealed class CharacterDetail
{
    public string Name { get; set; }

    [JsonConverter(typeof(AvailableClassesConverter))]
    public AvailableClasses Class { get; set; }

    public List<string> Specializations { get; set; }
}