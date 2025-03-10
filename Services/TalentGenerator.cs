using ArchonConfigUpdater.Models;
using ArchonConfigUpdater.Services.Contracts;
using ArchonConfigUpdater.Services.Utility;

namespace ArchonConfigUpdater.Services;

public class TalentGenerator(ITalentSource talentSource) : ITalentGenerator
{
    private const string RaidIdentifierPrefix = "R";
    private const string MythicPlusIdentifierPrefix = "M+";


    public async Task<List<Talent>> GenerateTalents(Config config)
    {
        var result = new List<Talent>();

        foreach (var character in config.Characters)
        {
            var specializations = character.Specializations;

            foreach (var spec in specializations)
            {
                var raidTalents = await ProcessRaidBosses(config, character, spec);

                result.AddRange(raidTalents);

                var dungeonTalents = await ProcessDungeons(config, character, spec);

                result.AddRange(dungeonTalents);
            }
        }

        return result;
    }

    private async Task<List<Talent>> ProcessRaidBosses(Config config, CharacterDetail character, string spec)
    {
        var list = new List<Talent>();
        var specIdentifier = ClassesWithSpecializations.GetSpecIndex(character.Class, spec);

        foreach (var boss in config.RaidBosses)
        {
            foreach (var difficulty in config.RaidDifficulties)
            {
                var identifier =
                    BuildTalentIdentifierUtility.BuildTalentIdentifier(RaidIdentifierPrefix, difficulty, boss);

                var talentString = await talentSource.GetRaidTalentSelectionAsync(character.Class.ToString(),
                    spec, difficulty, boss);

                if (string.IsNullOrEmpty(talentString))
                {
                    continue;
                }


                list.Add(new Talent
                {
                    Name = identifier, TalentSelection = talentString, Class = character.Class, Specialization = spec,
                    SpecIdentifier = specIdentifier
                });
            }
        }

        return list;
    }

    private async Task<List<Talent>> ProcessDungeons(Config config, CharacterDetail character, string spec)
    {
        var list = new List<Talent>();
        var specIdentifier = ClassesWithSpecializations.GetSpecIndex(character.Class, spec);

        foreach (var dungeon in config.Dungeons)
        {
            var identifier =
                BuildTalentIdentifierUtility.BuildTalentIdentifier(MythicPlusIdentifierPrefix, string.Empty, dungeon);

            var talentString = await talentSource.GetDungeonTalentSelectionAsync(character.Class.ToString(),
                spec, string.Empty, dungeon);

            if (string.IsNullOrEmpty(talentString))
            {
                continue;
            }

            list.Add(new Talent
            {
                Name = identifier, TalentSelection = talentString, Class = character.Class, Specialization = spec,
                SpecIdentifier = specIdentifier
            });
        }

        return list;
    }
}