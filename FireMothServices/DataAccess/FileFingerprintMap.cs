namespace RiotClub.FireMoth.Services.DataAccess
{
    using CsvHelper.Configuration;

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
            this.Map(fingerprint => fingerprint.FileInfo.DirectoryName)
                .Name("DirectoryName")
                .Index(0);
            this.Map(fingerprint => fingerprint.FileInfo.Name)
                .Name("Name")
                .Index(1);
            this.Map(fingerprint => fingerprint.FileInfo.Length)
                .Name("Length")
                .Index(2);
            this.Map(fingerprint => fingerprint.Base64Hash)
                .Name("Base64Hash")
                .Index(3);
        }
    }
}
