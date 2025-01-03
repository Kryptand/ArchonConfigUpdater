using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services;

public class UpdateTalentLoadoutExTalentsService
{
    public void UpdateTalents(Config config, List<Talent> talents)
    {
        var currentTalents = File.ReadAllText(config.OutputPath);
        var talentLoadoutEx = TalentLoadoutAdapter.FromCustomFormat(currentTalents);

        var talentsByClassAndSpec = talents.GroupBy(t => t.Class).ToDictionary(g => g.Key,
            g => g.GroupBy(t => t.Specialization).ToDictionary(g => g.Key, g => g.ToList()));

        foreach (var classEntry in talentsByClassAndSpec)
        {
            var className = classEntry.Key.ToString();
            foreach (var specEntry in classEntry.Value)
            {
                var specName = specEntry.Key;

                var specIndex = ClassesWithSpecializations.GetSpecIndex(classEntry.Key, specName);

                var talentsForSpec = specEntry.Value;

                var talentLoadoutExTalents = talentsForSpec.Select(t => new TalentLoadoutExTalent
                    { Name = t.Name, Text = t.TalentSelection, Icon = 0 }).ToList();

                talentLoadoutEx.UpsertGeneratedTalents(className, specIndex, talentLoadoutExTalents);
            }
        }

        File.WriteAllText(config.OutputPath, TalentLoadoutAdapter.ToCustomFormat(talentLoadoutEx));
    }
}