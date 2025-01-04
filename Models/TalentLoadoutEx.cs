namespace ArchonConfigUpdater.Models;

public class TalentLoadoutEx
{
    private readonly string generatedTalentSuffix = "_ARCT";
    public Option Option { get; set; }
    public Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> ClassTalents { get; set; }

    private Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> AddTalent(string className, int specIndex,
        TalentLoadoutExTalent talentLoadoutExTalent)
    {
        EnsureClassAndSpecExist(className, specIndex);
        ClassTalents[className][specIndex].Add(talentLoadoutExTalent);
        return ClassTalents;
    }

    private Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> AddManyTalents(string className,
        int specIndex, List<TalentLoadoutExTalent> talents)
    {
        EnsureClassAndSpecExist(className, specIndex);
        var generatedTalents = talents.Select(talent =>
        {
            talent.Name += generatedTalentSuffix;
            return talent;
        }).ToList();

        ClassTalents[className][specIndex].AddRange(generatedTalents);
        return ClassTalents;
    }

    private Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> RemoveGeneratedTalents(string className,
        int specIndex)
    {
        if (ClassTalents.ContainsKey(className) && ClassTalents[className].ContainsKey(specIndex))
        {
            ClassTalents[className][specIndex] = ClassTalents[className][specIndex]
                .Where(talent => !talent.Name.EndsWith(generatedTalentSuffix)).ToList();
        }

        return ClassTalents;
    }

    public Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> UpsertGeneratedTalents(string className,
        int specIndex, List<TalentLoadoutExTalent> talents)
    {
        var classNameUpper = className.ToUpper();
        RemoveGeneratedTalents(classNameUpper, specIndex);
        AddManyTalents(classNameUpper, specIndex, talents);
        return ClassTalents;
    }

    private void EnsureClassAndSpecExist(string className, int specIndex)
    {
        if (!ClassTalents.ContainsKey(className))
        {
            ClassTalents[className] = new Dictionary<int, List<TalentLoadoutExTalent>>();
        }

        if (!ClassTalents[className].ContainsKey(specIndex))
        {
            ClassTalents[className][specIndex] = new List<TalentLoadoutExTalent>();
        }
    }
}