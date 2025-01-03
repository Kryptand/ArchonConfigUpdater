namespace ArchonConfigUpdater.Models;

public enum AvailableClasses
{
    Warrior,
    Paladin,
    Hunter,
    Rogue,
    Priest,
    DeathKnight,
    Shaman,
    Mage,
    Warlock,
    Monk,
    Druid,
    DemonHunter,
    Evoker
}

public class Specialization
{
    public string Name { get; set; }
    public AvailableClasses Class { get; set; }

    public int SpecIndex { get; set; }
}

public static class ClassesWithSpecializations
{
    public static Dictionary<AvailableClasses, List<Specialization>> Specializations { get; set; } = new()
    {
        {
            AvailableClasses.Warrior, new List<Specialization>
            {
                new() { Name = "Arms", Class = AvailableClasses.Warrior, SpecIndex = 1 },
                new() { Name = "Fury", Class = AvailableClasses.Warrior, SpecIndex = 2 },
                new() { Name = "Protection", Class = AvailableClasses.Warrior, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Paladin, new List<Specialization>
            {
                new() { Name = "Holy", Class = AvailableClasses.Paladin, SpecIndex = 1 },
                new() { Name = "Protection", Class = AvailableClasses.Paladin, SpecIndex = 2 },
                new() { Name = "Retribution", Class = AvailableClasses.Paladin, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Hunter, new List<Specialization>
            {
                new() { Name = "Beast Mastery", Class = AvailableClasses.Hunter, SpecIndex = 1 },
                new() { Name = "Marksmanship", Class = AvailableClasses.Hunter, SpecIndex = 2 },
                new() { Name = "Survival", Class = AvailableClasses.Hunter, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Rogue, new List<Specialization>
            {
                new() { Name = "Assassination", Class = AvailableClasses.Rogue, SpecIndex = 1 },
                new() { Name = "Combat", Class = AvailableClasses.Rogue, SpecIndex = 2 },
                new() { Name = "Subtlety", Class = AvailableClasses.Rogue, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Priest, new List<Specialization>
            {
                new() { Name = "Discipline", Class = AvailableClasses.Priest, SpecIndex = 1 },
                new() { Name = "Holy", Class = AvailableClasses.Priest, SpecIndex = 2 },
                new() { Name = "Shadow", Class = AvailableClasses.Priest, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.DeathKnight, new List<Specialization>
            {
                new() { Name = "Blood", Class = AvailableClasses.DeathKnight, SpecIndex = 1 },
                new() { Name = "Frost", Class = AvailableClasses.DeathKnight, SpecIndex = 2 },
                new() { Name = "Unholy", Class = AvailableClasses.DeathKnight, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Shaman, new List<Specialization>
            {
                new() { Name = "Elemental", Class = AvailableClasses.Shaman, SpecIndex = 1 },
                new() { Name = "Enhancement", Class = AvailableClasses.Shaman, SpecIndex = 2 },
                new() { Name = "Restoration", Class = AvailableClasses.Shaman, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Mage, new List<Specialization>
            {
                new() { Name = "Arcane", Class = AvailableClasses.Mage, SpecIndex = 1 },
                new() { Name = "Fire", Class = AvailableClasses.Mage, SpecIndex = 2 },
                new() { Name = "Frost", Class = AvailableClasses.Mage, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Warlock, new List<Specialization>
            {
                new() { Name = "Affliction", Class = AvailableClasses.Warlock, SpecIndex = 1 },
                new() { Name = "Demonology", Class = AvailableClasses.Warlock, SpecIndex = 2 },
                new() { Name = "Destruction", Class = AvailableClasses.Warlock, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Monk, new List<Specialization>
            {
                new() { Name = "Brewmaster", Class = AvailableClasses.Monk, SpecIndex = 1 },
                new() { Name = "Mistweaver", Class = AvailableClasses.Monk, SpecIndex = 2 },
                new() { Name = "Windwalker", Class = AvailableClasses.Monk, SpecIndex = 3 }
            }
        },
        {
            AvailableClasses.Druid, new List<Specialization>
            {
                new() { Name = "Balance", Class = AvailableClasses.Druid, SpecIndex = 1 },
                new() { Name = "Feral", Class = AvailableClasses.Druid, SpecIndex = 2 },
                new() { Name = "Guardian", Class = AvailableClasses.Druid, SpecIndex = 3 },
                new() { Name = "Restoration", Class = AvailableClasses.Druid, SpecIndex = 4 }
            }
        },
        {
            AvailableClasses.DemonHunter, new List<Specialization>
            {
                new() { Name = "Havoc", Class = AvailableClasses.DemonHunter, SpecIndex = 1 },
                new() { Name = "Vengeance", Class = AvailableClasses.DemonHunter, SpecIndex = 2 }
            }
        },
        {
            AvailableClasses.Evoker, new List<Specialization>
            {
                new() { Name = "Devastation", Class = AvailableClasses.Evoker, SpecIndex = 1 },
                new() { Name = "Preservation", Class = AvailableClasses.Evoker, SpecIndex = 2 }
            }
        }
    };

    public static List<Specialization> GetSpecsForClass(AvailableClasses Class)
    {
        return Specializations[Class];
    }

    public static int GetSpecIndex(AvailableClasses Class, string SpecName)
    {
        return Specializations[Class].Find(x => x.Name.ToLower() == SpecName.ToLower()).SpecIndex;
    }
}