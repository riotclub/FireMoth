namespace RiotClub.FireMoth.Services.Output
{
    using RiotClub.FireMoth.Services.FileScanning;

    public interface IScanResultStreamWriter
    {
        public void WriteAllAsync(
            ScanResult scanResult, OutputDuplicateFileFingerprintsOption outputOption);
    }
}
