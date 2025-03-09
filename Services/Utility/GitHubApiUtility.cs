using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

namespace ArchonConfigUpdater.Services.Utility;

public class GitHubApiUtility
{
    private readonly HttpClient _httpClient;
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;
    private readonly string _currentVersion;
    
    public GitHubApiUtility(string repositoryOwner, string repositoryName, string currentVersion)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ArchonConfigUpdater");
        _repositoryOwner = repositoryOwner;
        _repositoryName = repositoryName;
        _currentVersion = currentVersion;
    }
    
    public async Task<GitHubRelease> GetLatestReleaseAsync()
    {
        var response = await _httpClient.GetAsync($"https://api.github.com/repos/{_repositoryOwner}/{_repositoryName}/releases/latest");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<GitHubRelease>();
    }
    
    public bool IsNewVersionAvailable(string latestVersion)
    {
        if (string.IsNullOrEmpty(latestVersion))
            return false;
            
        // Remove 'v' prefix if present
        var currentVersionClean = _currentVersion.TrimStart('v');
        var latestVersionClean = latestVersion.TrimStart('v');
        
        // Handle alpha/beta tags
        string currentVersionCore = currentVersionClean;
        string latestVersionCore = latestVersionClean;
        
        // Extract the core version without pre-release tags for comparison
        if (currentVersionClean.Contains("-"))
        {
            currentVersionCore = currentVersionClean.Split('-')[0];
        }
        
        if (latestVersionClean.Contains("-"))
        {
            latestVersionCore = latestVersionClean.Split('-')[0];
        }
        
        // Parse core versions
        if (Version.TryParse(currentVersionCore, out var currentParsedVersion) && 
            Version.TryParse(latestVersionCore, out var latestParsedVersion))
        {
            // If core versions are different, compare those
            if (latestParsedVersion != currentParsedVersion)
            {
                return latestParsedVersion > currentParsedVersion;
            }
            
            // If core versions are the same, compare pre-release tags
            // No prerelease tag is considered newer than any prerelease tag
            if (!latestVersionClean.Contains("-") && currentVersionClean.Contains("-"))
            {
                return true; // Stable release is newer than prerelease
            }
            
            if (latestVersionClean.Contains("-") && !currentVersionClean.Contains("-"))
            {
                return false; // Prerelease is not newer than stable
            }
            
            // If both have prerelease tags, compare alphabetically (simple approach)
            if (latestVersionClean.Contains("-") && currentVersionClean.Contains("-"))
            {
                var currentPrerelease = currentVersionClean.Split('-')[1];
                var latestPrerelease = latestVersionClean.Split('-')[1];
                
                return string.Compare(latestPrerelease, currentPrerelease, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }
        
        return false;
    }
    
    public async Task<string> DownloadReleaseAssetAsync(GitHubReleaseAsset asset, string destinationPath)
    {
        var response = await _httpClient.GetAsync(asset.BrowserDownloadUrl);
        response.EnsureSuccessStatusCode();
        
        var tempFilePath = Path.Combine(destinationPath, asset.Name);
        await using var fileStream = File.Create(tempFilePath);
        await response.Content.CopyToAsync(fileStream);
        
        return tempFilePath;
    }
    
    public GitHubReleaseAsset GetAppropriateAsset(IEnumerable<GitHubReleaseAsset> assets)
    {
        string osId;
        string architecture;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osId = "windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osId = "macos";
        }
        else
        {
            throw new PlatformNotSupportedException("The current platform is not supported for updates.");
        }
        
        architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("The current CPU architecture is not supported for updates.")
        };
        
        foreach (var asset in assets)
        {
            if (asset.Name.Contains(osId, StringComparison.OrdinalIgnoreCase) && 
                asset.Name.Contains(architecture, StringComparison.OrdinalIgnoreCase))
            {
                return asset;
            }
        }
        
        throw new InvalidOperationException("No compatible asset found for the current platform.");
    }
}

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }
    
    [JsonPropertyName("assets")]
    public List<GitHubReleaseAsset> Assets { get; set; }
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }
}

public class GitHubReleaseAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; }
} 