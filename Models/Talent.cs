namespace ArchonConfigUpdater.Models;

public class Talent
{
    public string Name { get; set; }
    public string TalentSelection { get; set; }
    public AvailableClasses Class { get; set; }
    public string Specialization { get; set; }
    public int SpecIdentifier { get; set; }
}