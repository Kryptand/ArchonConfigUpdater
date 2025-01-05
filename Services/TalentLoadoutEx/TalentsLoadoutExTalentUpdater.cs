using ArchonConfigUpdater.Models;
using ArchonConfigUpdater.Services.Contracts;

namespace ArchonConfigUpdater.Services.TalentLoadoutEx;

public class TalentsLoadoutExTalentUpdater : ITalentUpdater
{
    public async Task UpdateTalentsAsync(Config config, List<Talent> talents)
    {
        var currentTalents = await ReadCurrentTalentsAsync(config);

        var talentLoadoutEx = TalentLoadoutAdapter.FromCustomFormat(currentTalents);

        var talentsByClassAndSpec = GroupTalentsByClassAndSpec(talents);

        foreach (var classEntry in talentsByClassAndSpec)
        {
            UpsertTalentsForClass(classEntry, talentLoadoutEx);
        }

        await WriteUpdatedTalentsAsync(config, talentLoadoutEx);
    }

    private static async Task<string> ReadCurrentTalentsAsync(Config config)
    {
        return await File.ReadAllTextAsync(config.OutputPath);
    }

    private static Dictionary<AvailableClasses, Dictionary<string, List<Talent>>> GroupTalentsByClassAndSpec(
        List<Talent> talents)
    {
        return talents.GroupBy(t => t.Class)
            .ToDictionary(g => g.Key, g => g.GroupBy(t => t.Specialization).ToDictionary(g => g.Key, g => g.ToList()));
    }

    private static void UpsertTalentsForClass(
        KeyValuePair<AvailableClasses, Dictionary<string, List<Talent>>> classEntry,
        Models.TalentLoadoutEx talentLoadoutEx)
    {
        var className = classEntry.Key.ToString();

        foreach (var specEntry in classEntry.Value)
        {
            var specName = specEntry.Key;
            
            var specIndex = ClassesWithSpecializations.GetSpecIndex(classEntry.Key, specName);
            
            var talentsForSpec = specEntry.Value;
            
            var talentLoadoutExTalents = talentsForSpec.Select(t => new TalentLoadoutExTalent
            {
                Name = t.Name,
                Text = t.TalentSelection,
                Icon = 0
            }).ToList();

            talentLoadoutEx.UpsertGeneratedTalents(className, specIndex, talentLoadoutExTalents);
        }
    }

    private static async Task WriteUpdatedTalentsAsync(Config config, Models.TalentLoadoutEx talentLoadoutEx)
    {
        await File.WriteAllTextAsync(config.OutputPath, TalentLoadoutAdapter.ToCustomFormat(talentLoadoutEx));
    }
}