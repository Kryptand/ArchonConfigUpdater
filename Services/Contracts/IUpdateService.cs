using System.Threading.Tasks;

namespace ArchonConfigUpdater.Services.Contracts;

public interface IUpdateService
{
    Task<bool> CheckForUpdatesAsync();
    Task<bool> UpdateApplicationAsync();
} 