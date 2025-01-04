using System.Text;
using ArchonConfigUpdater.Models;

namespace ArchonConfigUpdater.Services.TalentLoadoutEx;

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
                inMultiLineBlock = true;
                currentEntry.Clear();
            }

            if (inMultiLineBlock)
            {
                currentEntry.Append(line);

                if (line.EndsWith("},"))
                {
                    inMultiLineBlock = false;
                    reformatted.AppendLine(currentEntry.ToString());
                    currentEntry.Clear();
                }
            }
            else
            {
                reformatted.AppendLine(line);
            }
        }

        return reformatted.ToString();
    }
    private static bool TryParseClassKey(string line, out string classKey)
    {
        classKey = null;

        if (!line.StartsWith("[\"") || !line.Contains("\"]")) return false;

        var endIndex = line.IndexOf("\"]");

        var key = line.Substring(2, endIndex - 2);

        if (Enum.TryParse<AvailableClasses>(key, true, out _))
        {
            classKey = key.ToUpper();
            return true;
        }

        if (key.Equals("OPTION", StringComparison.OrdinalIgnoreCase))
        {
            classKey = "OPTION";
            return true;
        }

        return false;
    }

    private static bool TryParseSpecKey(string line, string currentClassKey, out int specId)
    {
        specId = 0;
        if (!line.StartsWith("[")) return false;

        var start = line.IndexOf('[') + 1;

        var length = line.IndexOf(']') - start;

        if (length <= 0 || !int.TryParse(line.Substring(start, length), out specId) ||
            currentClassKey == "OPTION") return false;

        return IsSpecKeyValidForClass(currentClassKey, specId);
    }

    private static TalentLoadoutExTalent ParseTalent(string line)
    {
        var talent = new TalentLoadoutExTalent();
        string[] attributes = line.Trim('{', '}', ',').Split(",");

        foreach (var attribute in attributes)
        {
            string[] keyValue = attribute.Split('=');
            if (keyValue.Length == 2)
            {
                var talentKey = keyValue[0].Trim().Trim('[', ']', '\"');
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

        return talent;
    }

    private static void ParseOption(string line, Models.TalentLoadoutEx talentLoadoutEx)
    {
        string[] attributes = line.Split("=");

        if (attributes.Length != 2) return;

        var optKey = attributes[0].Trim().Trim('\"');
        var optValue = attributes[1].Trim().Trim(',', ' ');

        if (!optKey.Equals("IsEnabledPvp", StringComparison.OrdinalIgnoreCase)) return;

        talentLoadoutEx.Option.IsEnabledPvp = bool.Parse(optValue.ToLower());
    }

    private static void EnsureClassExists(Models.TalentLoadoutEx talentLoadoutEx, string classKey)
    {
        if (talentLoadoutEx.ClassTalents.ContainsKey(classKey)) return;

        talentLoadoutEx.ClassTalents[classKey] = new Dictionary<int, List<TalentLoadoutExTalent>>();
    }

    private static void EnsureSpecExists(Models.TalentLoadoutEx talentLoadoutEx, string classKey, int specId)
    {
        if (talentLoadoutEx.ClassTalents[classKey].ContainsKey(specId)) return;

        talentLoadoutEx.ClassTalents[classKey][specId] = new List<TalentLoadoutExTalent>();
    }

    private static bool IsSpecKeyValidForClass(string className, int specKey)
    {
        if (!Enum.TryParse<AvailableClasses>(className, true, out var availableClass)) return false;

        var specializations = ClassesWithSpecializations.GetSpecsForClass(availableClass);

        return specializations.Any(spec => spec.SpecIndex == specKey);
    }
    public static Models.TalentLoadoutEx FromCustomFormat(string customFormat)
    {
        customFormat = ReformatToSingleLine(customFormat);

        var talentLoadoutEx = new Models.TalentLoadoutEx
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

            if (TryParseClassKey(trimmedLine, out var classKey))
            {
                currentClassKey = classKey;
                currentSpecKey = null;
                EnsureClassExists(talentLoadoutEx, currentClassKey);
            }
            else if (currentClassKey != null && TryParseSpecKey(trimmedLine, currentClassKey, out var specId))
            {
                currentSpecKey = specId;
                EnsureSpecExists(talentLoadoutEx, currentClassKey, currentSpecKey.Value);
            }
            else if (currentClassKey != null && currentSpecKey.HasValue && trimmedLine.StartsWith("{"))
            {
                var talent = ParseTalent(trimmedLine);
                talentLoadoutEx.ClassTalents[currentClassKey][currentSpecKey.Value].Add(talent);
            }
            else if (currentClassKey == "OPTION")
            {
                ParseOption(trimmedLine, talentLoadoutEx);
            }
        }

        return talentLoadoutEx;
    }

    public static string ToCustomFormat(Models.TalentLoadoutEx talentLoadoutEx)
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
                if (!IsSpecKeyValidForClass(classKey, specEntry.Key)) continue;
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

            lines.Add("},");
        }

        lines.Add(
            $"[\"OPTION\"] = {{ [\"IsEnabledPvp\"] = {talentLoadoutEx.Option.IsEnabledPvp.ToString().ToLower()} }},");
        lines.Add("}");

        return string.Join(Environment.NewLine, lines);
    }
}