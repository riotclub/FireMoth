using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotClub.FireMoth.Services.FileScanning
{
    public class ScanError
    {
        public string Path { get; set; }

        public Exception? Exception { get; set; }


    }
}
