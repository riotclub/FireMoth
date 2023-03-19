namespace RiotClub.FireMoth.Services.Orchestrator
{
    using RiotClub.FireMoth.Services.FileScanning;

    internal interface IScanOrchestrator
    {
        ScanResult ScanDirectory(IScanOptions scanOptions);
    }
}
