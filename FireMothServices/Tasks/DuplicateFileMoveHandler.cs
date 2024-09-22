namespace RiotClub.FireMoth.Services.Tasks;

using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

public class DuplicateFileMoveHandler : ITaskHandler
{
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly DuplicateFileHandlingOptions _duplicateFileHandlingOptions;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DuplicateFileHandler> _logger;
    
    public Task RunTaskAsync()
    {
        throw new System.NotImplementedException();
    }
}