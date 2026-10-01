using FolderSyncApp.Interfaces; 

namespace FolderSyncApp;

public class Synchroniser
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public Synchroniser(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public void Sync(string source, string replica)
    {
        if (!_fileSystem.DirectoryExists(source))
            throw new DirectoryNotFoundException(
                $"Source directory does not exist: {source}");

        _fileSystem.CreateDirectory(replica);

        CreateMissingDirectories(source, replica);
        CopyNewOrChangedFiles(source, replica);
        RemoveObsoleteFiles(source, replica);
        RemoveObsoleteDirectories(source, replica);
    }

    private void CreateMissingDirectories(string source, string replica)
    {
        foreach (var sourceDirectory in _fileSystem.GetDirectories(source))
        {
            var relativePath = Path.GetRelativePath(source, sourceDirectory);
            var replicaDirectory = Path.Combine(replica, relativePath);

            if (!_fileSystem.DirectoryExists(replicaDirectory))
            {
                _fileSystem.CreateDirectory(replicaDirectory);
                _logger.Log($"Created directory: {replicaDirectory}");
            }
        }
    }

    private void CopyNewOrChangedFiles(string source, string replica)
    {
        foreach (var sourceFile in _fileSystem.GetFiles(source))
        {
            var relativePath = Path.GetRelativePath(source, sourceFile);
            var replicaFile = Path.Combine(replica, relativePath);

            if (!_fileSystem.FileExists(replicaFile))
            {
                _fileSystem.CopyFile(sourceFile, replicaFile);
                _logger.Log($"Copied file: {relativePath}");
            }
            else if (!_fileSystem.FilesAreEqual(sourceFile, replicaFile))
            {
                _fileSystem.CopyFile(sourceFile, replicaFile);
                _logger.Log($"Updated file: {relativePath}");
            }
        }
    }

    private void RemoveObsoleteFiles(string source, string replica)
    {
        foreach (var replicaFile in _fileSystem.GetFiles(replica))
        {
            var relativePath = Path.GetRelativePath(replica, replicaFile);
            var sourceFile = Path.Combine(source, relativePath);

            if (!_fileSystem.FileExists(sourceFile))
            {
                _fileSystem.DeleteFile(replicaFile);
                _logger.Log($"Removed file: {relativePath}");
            }
        }
    }

    private void RemoveObsoleteDirectories(string source, string replica)
    {
        foreach (var replicaDirectory in _fileSystem
            .GetDirectories(replica)
            .OrderByDescending(x => x.Length))
        {
            var relativePath = Path.GetRelativePath(replica, replicaDirectory);
            var sourceDirectory = Path.Combine(source, relativePath);

            if (!_fileSystem.DirectoryExists(sourceDirectory))
            {
                _fileSystem.DeleteDirectory(replicaDirectory, true);
                _logger.Log($"Removed directory: {relativePath}");
            }
        }
    }
}