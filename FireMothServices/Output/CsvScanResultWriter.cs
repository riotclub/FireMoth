namespace RiotClub.FireMoth.Services.Output
{
    using RiotClub.FireMoth.Services.FileScanning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CsvScanResultWriter : IScanResultStreamWriter
    {
        public void WriteAllAsync(ScanResult scanResult, OutputDuplicateFileFingerprintsOption outputOption)
        {
            throw new NotImplementedException();
        }
    }
}
