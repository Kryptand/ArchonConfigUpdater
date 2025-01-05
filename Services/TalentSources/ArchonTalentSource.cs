using System.Net;
using ArchonConfigUpdater.Services.Contracts;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ArchonConfigUpdater.Services.TalentSources;

public class ArchonTalentSource(ILogger<ArchonTalentSource> logger) : ITalentSource
{
    private readonly string _archonUrl =
        "https://www.archon.gg/wow/builds/{spec}/{class}/{contentType}/{tier}/{difficulty}/{encounter}";

    private readonly HttpClient _client = new();
    private readonly string lastWeekIdentifier = "last-week";
    private readonly string thisWeekIdentifier = "this-week";

    public async Task<string> GetDungeonTalentSelectionAsync(string className, string spec, string difficulty,
        string encounter)
    {
        var dayOfWeek = DateTime.Now.DayOfWeek;

        var isResetDay = dayOfWeek == DayOfWeek.Wednesday;

        var timeSpan = isResetDay ? lastWeekIdentifier : thisWeekIdentifier;

        var result = await GetTalentSelectionAsync(_archonUrl + "/" + timeSpan, className, spec, "mythic-plus",
            "overview/10",
            difficulty, encounter);

        if (!string.IsNullOrEmpty(result))
        {
            return result;
        }

        if (timeSpan == lastWeekIdentifier)
        {
            result = await GetTalentSelectionAsync(_archonUrl + "/" + thisWeekIdentifier, className, spec,
                "mythic-plus", "overview/10",
                difficulty, encounter);

            return result;
        }

        result = await GetTalentSelectionAsync(_archonUrl + "/" + lastWeekIdentifier, className, spec, "mythic-plus",
            "overview/10",
            difficulty, encounter);

        return result;
    }

    public Task<string> GetRaidTalentSelectionAsync(string className, string spec, string difficulty, string encounter)
    {
        return GetTalentSelectionAsync(_archonUrl, className, spec, "raid", "overview", difficulty, encounter);
    }

    private async Task<string> GetTalentSelectionAsync(string url, string className, string spec, string contentType,
        string tier, string difficulty, string encounter)
    {
        className = MapClassName(className);
        url = PrepareUrl(url, className, spec, contentType, tier, difficulty, encounter);

        try
        {
            var html = await _client.GetStringAsync(url);

            var doc = new HtmlDocument();


            doc.LoadHtml(html);

            var linkNode =
                doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'wowhead.com/talent-calc/blizzard/')]");

            var result = linkNode?.GetAttributeValue("href", string.Empty);

            result = result?.Replace("https://www.wowhead.com/talent-calc/blizzard/", string.Empty);

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Could not find talent string");
            }

            logger.LogDebug(
                $"Talent string for {className} {spec} {contentType} {tier} {difficulty} {encounter} has been loaded successfully");

            return result;
        }
        catch (Exception ex)
        {
            // if the error is 500 this means that the current boss has not enough data so we can ignore it
            if (ex is HttpRequestException { StatusCode: HttpStatusCode.InternalServerError })
            {
                logger.LogWarning(
                    $"Could not find talent string for {className} {spec} {contentType} {tier} {difficulty} {encounter}");

                return string.Empty;
            }
        }

        return string.Empty;
    }

    private static string MapClassName(string className)
    {
        return className switch
        {
            "DeathKnight" => "death-knight",
            "DemonHunter" => "demon-hunter",
            _ => className.ToLowerInvariant()
        };
    }

    private static string PrepareUrl(string url, string className, string spec, string contentType, string tier,
        string difficulty, string encounter)
    {
        var classUrl = className.ToLowerInvariant();
        var specUrl = spec.ToLowerInvariant();
        var contentTypeUrl = contentType.ToLowerInvariant();
        var difficultyUrl = difficulty.ToLowerInvariant();
        var tierUrl = tier.ToLowerInvariant();
        var encounterUrl = encounter.ToLowerInvariant();

        url = url.Replace("{spec}", specUrl)
            .Replace("{class}", classUrl)
            .Replace("{contentType}", contentTypeUrl)
            .Replace("{encounter}", encounterUrl)
            .Replace("{tier}", tierUrl);

        if (!string.IsNullOrEmpty(difficulty))
        {
            url = url.Replace("{difficulty}", difficultyUrl);
        }
        else
        {
            url = url.Replace("/{difficulty}", string.Empty);
        }

        return url;
    }
}