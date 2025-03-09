using ArchonConfigUpdater.Services;
using ArchonConfigUpdater.Services.Contracts;
using ArchonConfigUpdater.Services.TalentLoadoutEx;
using ArchonConfigUpdater.Services.TalentSources;
using ArchonConfigUpdater.Services.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArchonConfigUpdater;

internal class Program
{
    private static async Task Main(string[] args)
    {
        bool checkUpdateOnly = args.Contains("--check-update");
        bool forceUpdate = args.Contains("--update");
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ParseSettingsUtility>();
                services.AddTransient<ITalentUpdater, TalentsLoadoutExTalentUpdater>();
                services.AddTransient<ITalentSource, ArchonTalentSource>();
                services.AddTransient<ITalentGenerator, TalentGenerator>();
                services.AddTransient<IUpdateService, UpdateService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            var config = host.Services.GetRequiredService<ParseSettingsUtility>().ParseFile("./settings.json");

            VerifyConfigUtility.VerifyConfig(config);

            // Handle command-line update flags
            if (checkUpdateOnly || forceUpdate)
            {
                var updateService = host.Services.GetRequiredService<IUpdateService>();
                var updateAvailable = await updateService.CheckForUpdatesAsync();
                
                if (updateAvailable)
                {
                    Console.WriteLine("An update is available.");
                    
                    if (forceUpdate)
                    {
                        Console.WriteLine("Downloading and installing update...");
                        var updateSuccessful = await updateService.UpdateApplicationAsync();
                        
                        if (updateSuccessful)
                        {
                            Console.WriteLine("Update downloaded and will be applied on exit.");
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.WriteLine("Update failed.");
                            Environment.Exit(1);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No updates available.");
                }
                
                if (checkUpdateOnly)
                {
                    Environment.Exit(0);
                }
            }
            
            // Check for updates if enabled in config
            if (config.Update?.AutoCheckForUpdates == true)
            {
                var updateService = host.Services.GetRequiredService<IUpdateService>();
                var updateAvailable = await updateService.CheckForUpdatesAsync();

                if (updateAvailable)
                {
                    if (config.Update.AutoInstallUpdates)
                    {
                        logger.LogInformation("Auto-installing update...");
                        var updateSuccessful = await updateService.UpdateApplicationAsync();
                        
                        if (updateSuccessful)
                        {
                            logger.LogInformation("Update will be applied when the application exits.");
                            Console.WriteLine("Update downloaded and will be applied on exit. Press any key to exit...");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Console.WriteLine("A new update is available. Would you like to install it now? (Y/N)");
                        var key = Console.ReadKey(true);
                        
                        if (key.Key == ConsoleKey.Y)
                        {
                            var updateSuccessful = await updateService.UpdateApplicationAsync();
                            
                            if (updateSuccessful)
                            {
                                logger.LogInformation("Update will be applied when the application exits.");
                                Console.WriteLine("Update downloaded and will be applied on exit. Press any key to exit...");
                                Console.ReadKey();
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }

            var talentGenerator = host.Services.GetRequiredService<ITalentGenerator>();

            var talents = await talentGenerator.GenerateTalents(config);

            var updateTalentsService = host.Services.GetRequiredService<ITalentUpdater>();

            await updateTalentsService.UpdateTalentsAsync(config, talents);

            logger.LogInformation("Exiting application.");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            logger.LogCritical(ex.StackTrace);

            logger.LogCritical("An error occurred. Exiting application.");
            Environment.Exit(1);
        }

        await host.RunAsync();
    }
}