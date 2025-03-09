using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using ArchonConfigUpdater.Services.Contracts;
using ArchonConfigUpdater.Services.Utility;
using Microsoft.Extensions.Logging;

namespace ArchonConfigUpdater.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly GitHubApiUtility _gitHubApiUtility;
    private GitHubRelease _latestRelease;
    
    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
        
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        _gitHubApiUtility = new GitHubApiUtility("Kryptand", "ArchonConfigUpdater", currentVersion);
    }
    
    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            _logger.LogInformation("Checking for updates...");
            
            _latestRelease = await _gitHubApiUtility.GetLatestReleaseAsync();
            
            var isNewVersionAvailable = _gitHubApiUtility.IsNewVersionAvailable(_latestRelease.TagName);
            
            if (isNewVersionAvailable)
            {
                _logger.LogInformation($"A new version {_latestRelease.TagName} is available! Current version: {Assembly.GetExecutingAssembly().GetName().Version}");
                return true;
            }
            
            _logger.LogInformation("You are using the latest version.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates.");
            return false;
        }
    }
    
    public async Task<bool> UpdateApplicationAsync()
    {
        if (_latestRelease == null)
        {
            var updateAvailable = await CheckForUpdatesAsync();
            if (!updateAvailable)
                return false;
        }
        
        try
        {
            _logger.LogInformation($"Downloading update {_latestRelease.TagName}...");
            
            var asset = _gitHubApiUtility.GetAppropriateAsset(_latestRelease.Assets);
            
            var tempDirectory = Path.Combine(Path.GetTempPath(), "ArchonConfigUpdater_Update");
            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);
                
            Directory.CreateDirectory(tempDirectory);
            
            var downloadPath = await _gitHubApiUtility.DownloadReleaseAssetAsync(asset, tempDirectory);
            _logger.LogInformation($"Downloaded update to {downloadPath}");
            
            // Extract the zip file
            var extractPath = Path.Combine(tempDirectory, "extracted");
            Directory.CreateDirectory(extractPath);
            ZipFile.ExtractToDirectory(downloadPath, extractPath);
            
            // Get the current application path
            var currentAppPath = Assembly.GetExecutingAssembly().Location;
            
            // If the file ends with .dll, we're likely running as a dotnet application
            // We need to find the corresponding executable
            if (currentAppPath.EndsWith(".dll"))
            {
                var directoryName = Path.GetDirectoryName(currentAppPath);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentAppPath);
                
                var possibleExe = Path.Combine(directoryName, fileNameWithoutExtension + ".exe");
                if (File.Exists(possibleExe))
                    currentAppPath = possibleExe;
            }
            
            var updateScript = CreateUpdateScript(extractPath, currentAppPath);
            
            // Start the update script as a detached process
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = updateScript,
                UseShellExecute = true,
                CreateNoWindow = true
            };
            
            System.Diagnostics.Process.Start(startInfo);
            
            _logger.LogInformation("Update has been downloaded and will be applied when the application exits.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application.");
            return false;
        }
    }
    
    private string CreateUpdateScript(string sourcePath, string targetAppPath)
    {
        var scriptExtension = Environment.OSVersion.Platform == PlatformID.Win32NT ? "bat" : "sh";
        var scriptPath = Path.Combine(Path.GetTempPath(), $"update_archon_config.{scriptExtension}");
        
        var targetDirectory = Path.GetDirectoryName(targetAppPath);
        var executableName = Path.GetFileName(targetAppPath);
        
        using var writer = new StreamWriter(scriptPath);
        
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            // Windows batch script
            writer.WriteLine("@echo off");
            writer.WriteLine("timeout /t 2 /nobreak > nul");
            writer.WriteLine($"xcopy /E /Y \"{sourcePath}\\*\" \"{targetDirectory}\\\"");
            writer.WriteLine($"start \"\" \"{Path.Combine(targetDirectory, executableName)}\"");
            writer.WriteLine("exit");
        }
        else
        {
            // Unix/Mac shell script
            writer.WriteLine("#!/bin/bash");
            writer.WriteLine("sleep 2");
            writer.WriteLine($"cp -R \"{sourcePath}/\"* \"{targetDirectory}/\"");
            writer.WriteLine($"chmod +x \"{Path.Combine(targetDirectory, executableName)}\"");
            writer.WriteLine($"open \"{Path.Combine(targetDirectory, executableName)}\"");
            writer.WriteLine("exit");
        }
        
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            // Make the script executable on Unix/Mac
            var chmodStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x {scriptPath}",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(chmodStartInfo)?.WaitForExit();
        }
        
        return scriptPath;
    }
} 