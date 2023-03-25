namespace RiotClub.FireMoth.Services.Orchestration;

using System.Threading.Tasks;
using FileScanning;

internal interface IScanOrchestrator
{
    // Task<ScanResult> ScanFilesAsync(IEnumerable<string> files);

    Task<ScanResult> ScanDirectoryAsync();
}