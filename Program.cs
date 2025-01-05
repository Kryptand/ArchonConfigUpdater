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
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ParseSettingsUtility>();
                services.AddTransient<ITalentUpdater, TalentsLoadoutExTalentUpdater>();
                services.AddTransient<ITalentSource, ArchonTalentSource>();
                services.AddTransient<ITalentGenerator, TalentGenerator>();
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

            var talentGenerator = host.Services.GetRequiredService<ITalentGenerator>();

            var talents = await talentGenerator.GenerateTalents(config);

            var updateTalentsService = host.Services.GetRequiredService<ITalentUpdater>();

            await updateTalentsService.UpdateTalentsAsync(config, talents);

            logger.LogInformation("Exiting application.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            logger.LogCritical(ex.StackTrace);

            logger.LogCritical("An error occurred. Exiting application.");
        }

        await host.RunAsync();
    }
}