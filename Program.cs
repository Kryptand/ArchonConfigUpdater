using ArchonConfigUpdater.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var config = new ParseSettings().ParseFile("./settings.json");

            var talentCache = new TalentCache("./talentCache.json");

            if (!talentCache.TryGetCachedTalents(out var talents))
            {
                var updateTalentsService = new ArchonTalentGenerator();
                talents = await updateTalentsService.GenerateTalents(config);
                talentCache.SaveTalentsToCache(talents);
            }

            var updateTalentsServicse = new UpdateTalentLoadoutExTalentsService();
            updateTalentsServicse.UpdateTalents(config, talents);

            Console.WriteLine("Talents updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("Failed to update talents");
        }
    }
}