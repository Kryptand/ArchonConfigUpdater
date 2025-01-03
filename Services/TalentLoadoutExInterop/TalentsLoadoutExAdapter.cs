using System.Text;
using System.Text.RegularExpressions;
using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services;

public class TalentLoadoutEx
{
    private readonly string generatedTalentSuffix = "_ARCT";
    public Option Option { get; set; }
    public Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> ClassTalents { get; set; }

    private Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> AddTalent(string className, int specIndex,
        TalentLoadoutExTalent talentLoadoutExTalent)
    {
        if (!ClassTalents.ContainsKey(className))
        {
            ClassTalents[className] = new Dictionary<int, List<TalentLoadoutExTalent>>();
        }

        if (!ClassTalents[className].ContainsKey(specIndex))
        {
            ClassTalents[className][specIndex] = new List<TalentLoadoutExTalent>();
        }

        ClassTalents[className][specIndex].Add(talentLoadoutExTalent);

        return ClassTalents;
    }

    private Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>> AddManyTalents(string className,
        int specIndex, List<TalentLoadoutExTalent> talents)
    {
        if (!ClassTalents.ContainsKey(className))
        {
            ClassTalents[className] = new Dictionary<int, List<TalentLoadoutExTalent>>();
        }

        if (!ClassTalents[className].ContainsKey(specIndex))
        {
            ClassTalents[className][specIndex] = new List<TalentLoadoutExTalent>();
        }

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
}

public class Option
{
    public bool IsEnabledPvp { get; set; }
}

public class TalentLoadoutExTalent
{
    public bool? IsExpanded { get; set; }
    public bool? IsInGroup { get; set; }
    public int Icon { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
}

public static class TalentLoadoutAdapter
{
    private static string ReformatToSingleLine(string multilineData)
    {
        var reformatted = new StringBuilder();
        string[] lines = multilineData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var inMultiLineBlock = false;
        var currentEntry = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (!inMultiLineBlock && line.StartsWith("{"))
            {
                // Starting a new multi-line block
                inMultiLineBlock = true;
                currentEntry.Clear();
            }

            if (inMultiLineBlock)
            {
                currentEntry.Append(line);

                // Detect end of multi-line block
                if (line.EndsWith("},"))
                {
                    inMultiLineBlock = false;
                    reformatted.AppendLine(currentEntry.ToString());
                    currentEntry.Clear();
                }
            }
            else
            {
                // Copy lines that are already single-line
                reformatted.AppendLine(line);
            }
        }

        return reformatted.ToString();
    }

    public static TalentLoadoutEx FromCustomFormat(string customFormat)
    {
        customFormat = ReformatToSingleLine(customFormat);

        var talentLoadoutEx = new TalentLoadoutEx
        {
            Option = new Option(),
            ClassTalents = new Dictionary<string, Dictionary<int, List<TalentLoadoutExTalent>>>()
        };

        string[] lines = customFormat.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        string currentClassKey = null;
        int? currentSpecKey = null;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Detect and parse class and spec identifiers
            if (trimmedLine.StartsWith("[\""))
            {
                var endIndex = trimmedLine.IndexOf("\"]");
                if (endIndex > 0)
                {
                    var key = trimmedLine.Substring(2, endIndex - 2);

                    if (Enum.TryParse<AvailableClasses>(key, true, out _))
                    {
                        currentClassKey = key.ToUpper();
                        currentSpecKey = null;
                        if (!talentLoadoutEx.ClassTalents.ContainsKey(currentClassKey))
                        {
                            talentLoadoutEx.ClassTalents[currentClassKey] =
                                new Dictionary<int, List<TalentLoadoutExTalent>>();
                        }
                    }
                    else if (key.Equals("OPTION", StringComparison.OrdinalIgnoreCase))
                    {
                        currentClassKey = "OPTION";
                    }
                }
            }
            else if (currentClassKey != null && trimmedLine.StartsWith("["))
            {
                var start = trimmedLine.IndexOf('[') + 1;
                var length = trimmedLine.IndexOf(']') - start;
                if (length > 0 && int.TryParse(trimmedLine.Substring(start, length), out var specId) &&
                    currentClassKey != "OPTION")
                {
                    if (IsSpecKeyValidForClass(currentClassKey, specId))
                    {
                        currentSpecKey = specId;
                        talentLoadoutEx.ClassTalents[currentClassKey][specId] = new List<TalentLoadoutExTalent>();
                    }
                }
            }
            else if (currentClassKey != null && currentSpecKey.HasValue && trimmedLine.StartsWith("{"))
            {
                // Parse talent information within this block
                var talent = new TalentLoadoutExTalent();
                string[] attributes = trimmedLine.Trim('{', '}', ',').Split(",");

                foreach (var attribute in attributes)
                {
                    string[] keyValue = attribute.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var talentKey = keyValue[0].Trim();
                        talentKey = talentKey.Trim('[', ']', '\"');
                        var talentValue = keyValue[1].Trim().Trim('\"');
                        talentValue = talentValue.Trim('[', ']');

                        switch (talentKey)
                        {
                            case "icon":
                                if (int.TryParse(talentValue, out var icon))
                                    talent.Icon = icon;
                                break;
                            case "name":
                                talent.Name = talentValue;
                                break;
                            case "text":
                                talent.Text = talentValue;
                                break;
                            case "isExpanded":
                                talent.IsExpanded = bool.Parse(talentValue.ToLower());
                                break;
                            case "isInGroup":
                                talent.IsInGroup = bool.Parse(talentValue.ToLower());
                                break;
                        }
                    }
                }

                talentLoadoutEx.ClassTalents[currentClassKey][currentSpecKey.Value].Add(talent);
            }
            else if (currentClassKey == "OPTION")
            {
                // If we're handling the "OPTION" section
                string[] attributes = trimmedLine.Split("=");
                if (attributes.Length == 2)
                {
                    var optKey = attributes[0].Trim().Trim('\"');
                    var optValue = attributes[1].Trim().Trim(',', ' ');

                    if (optKey.Equals("IsEnabledPvp", StringComparison.OrdinalIgnoreCase))
                    {
                        talentLoadoutEx.Option.IsEnabledPvp = bool.Parse(optValue.ToLower());
                    }
                }
            }
        }

        return talentLoadoutEx;
    }

    private static TalentLoadoutExTalent ParseTalent(string value)
    {
        var talent = new TalentLoadoutExTalent();
        var talentRegex = new Regex(@"\{([^}]*)\}", RegexOptions.Singleline);
        var match = talentRegex.Match(value);

        if (match.Success)
        {
            var talentAttributes = match.Groups[1].Value;
            var attributeRegex = new Regex(@"\[\""(\w+)\""\] = (true|false|\d+|""[^""]*""),?", RegexOptions.Singleline);
            var attributeMatches = attributeRegex.Matches(talentAttributes);

            foreach (Match attributeMatch in attributeMatches)
            {
                var key = attributeMatch.Groups[1].Value;
                var val = attributeMatch.Groups[2].Value;

                switch (key)
                {
                    case "icon":
                        talent.Icon = int.Parse(val);
                        break;
                    case "name":
                        talent.Name = val.Trim('"');
                        break;
                    case "text":
                        talent.Text = val.Trim('"');
                        break;
                    case "isExpanded":
                        talent.IsExpanded = bool.Parse(val.ToLower());
                        break;
                    case "isInGroup":
                        talent.IsInGroup = bool.Parse(val.ToLower());
                        break;
                }
            }
        }

        return talent;
    }

    private static bool IsSpecKeyValidForClass(string className, int specKey)
    {
        if (Enum.TryParse<AvailableClasses>(className, true, out var availableClass))
        {
            var specializations = ClassesWithSpecializations.GetSpecsForClass(availableClass);
            return specializations.Any(spec => spec.SpecIndex == specKey);
        }

        return false;
    }

    // TalentLoadoutAdapter.cs
    public static string ToCustomFormat(TalentLoadoutEx talentLoadoutEx)
    {
        var lines = new List<string>
        {
            "TalentLoadoutEx = {"
        };

        foreach (var classKey in talentLoadoutEx.ClassTalents.Keys.Distinct())
        {
            lines.Add($"[\"{classKey.ToUpper()}\"] = {{");

            foreach (var specEntry in talentLoadoutEx.ClassTalents[classKey])
            {
                if (IsSpecKeyValidForClass(classKey, specEntry.Key))
                {
                    lines.Add($"  [{specEntry.Key}] = {{");

                    foreach (var talent in specEntry.Value)
                    {
                        var expanded = talent.IsExpanded.HasValue
                            ? $"[\"isExpanded\"] = {talent.IsExpanded.ToString().ToLower()}, "
                            : "";
                        var inGroup = talent.IsInGroup.HasValue
                            ? $"[\"isInGroup\"] = {talent.IsInGroup.ToString().ToLower()}, "
                            : "";
                        var text = !string.IsNullOrEmpty(talent.Text) ? $"[\"text\"] = \"{talent.Text}\", " : "";

                        lines.Add(
                            $"    {{ {expanded}{inGroup}[\"icon\"] = {talent.Icon}, [\"name\"] = \"{talent.Name}\", {text}}},");
                    }

                    lines.Add("  },");
                }
            }

            lines.Add("},");
        }

        lines.Add(
            $"[\"OPTION\"] = {{ [\"IsEnabledPvp\"] = {talentLoadoutEx.Option.IsEnabledPvp.ToString().ToLower()} }},");
        lines.Add("}");

        return string.Join(Environment.NewLine, lines);
    }
}