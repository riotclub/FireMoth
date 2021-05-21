namespace RiotClub.FireMoth.Services.DataAccess
{
    using CsvHelper.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A mapping between an <see cref="IFileInfo"/> and CSV headers.
    /// </summary>
    /// <seealso cref="ClassMap"/>
    public class FileFingerprintMap : ClassMap<IFileFingerprint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFingerprintMap"/> class.
        /// </summary>
        public FileFingerprintMap()
        {
            this.Map(fingerprint => fingerprint.FileInfo.DirectoryName).Name("DirectoryName");
            this.Map(fingerprint => fingerprint.FileInfo.Name).Name("Name");
            this.Map(fingerprint => fingerprint.FileInfo.Length).Name("Length");
            this.Map(fingerprint => fingerprint.Base64Hash).Name("Base64Hash");
        }
    }
}
