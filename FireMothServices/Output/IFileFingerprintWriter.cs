using System.Collections.Generic;
using System.Threading.Tasks;
using RiotClub.FireMoth.Services.Repository;

namespace RiotClub.FireMoth.Services.Output
{
    /// <summary>
    /// Defines the public interface for a class that implements 
    /// </summary>
    public interface IFileFingerprintWriter
    {
        /// <summary>
        /// Writes the provided collection of file fingerprints.
        /// </summary>
        /// <param name="fileFingerprints">An <see cref="IEnumerable{IFileFingerprint}"/> collection to write.</param>
        public Task WriteFileFingerprintsAsync(IEnumerable<IFileFingerprint> fileFingerprints);
    }
}
