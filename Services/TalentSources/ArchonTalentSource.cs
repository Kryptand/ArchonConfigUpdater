using System.Net;
using ArchonConfigUpdater.Services.Contracts;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ArchonConfigUpdater.Services.TalentSources;

public class ArchonTalentSource : ITalentSource
{
    private readonly string _archonUrl =
        "https://www.archon.gg/wow/builds/{spec}/{class}/{contentType}/{tier}/{difficulty}/{encounter}";

    private readonly HttpClient _client = new();
    private readonly ILogger<ArchonTalentSource> _logger;

    public ArchonTalentSource(ILogger<ArchonTalentSource> logger)
    {
        _logger = logger;
    }

    public Task<string> GetDungeonTalentSelectionAsync(string className, string spec, string difficulty,
        string encounter)
    {
        return GetTalentSelectionAsync(_archonUrl, className + "/" + "last-week", spec, "mythic-plus", "overview/10",
            difficulty, encounter);
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

            _logger.LogDebug($"Talent string for {className} {spec} {contentType} {tier} {difficulty} {encounter} has been loaded successfully");
   
            doc.LoadHtml(html);

            var linkNode =
                doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'wowhead.com/talent-calc/blizzard/')]");

            var result = linkNode?.GetAttributeValue("href", string.Empty);

            result = result?.Replace("https://www.wowhead.com/talent-calc/blizzard/", string.Empty);

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Could not find talent string");
            }


            return result;
        }
        catch (Exception ex)
        {
            // if the error is 500 this means that the current boss has not enough data so we can ignore it
            if (ex is HttpRequestException { StatusCode: HttpStatusCode.InternalServerError })
            {
                _logger.LogWarning(
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