using System.Security.Cryptography;
using FolderSyncApp.Interfaces;

namespace FolderSyncApp;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public IEnumerable<string> GetFiles(string path)
    {
        return Directory.GetFiles(path, "*", SearchOption.AllDirectories);
    }

    public IEnumerable<string> GetDirectories(string path)
    {
        return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void CopyFile(string source, string destination)
    {
        File.Copy(source, destination, true);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        Directory.Delete(path, recursive);
    }

    public bool FilesAreEqual(string source, string replica)
    {
        var sourceFileInfo = new FileInfo(source);
        var replicaFileInfo = new FileInfo(replica);

        if (sourceFileInfo.Length != replicaFileInfo.Length)
            return false;

        using var sourceStream = File.OpenRead(source);
        using var replicaStream = File.OpenRead(replica);

        return SHA256.HashData(sourceStream).SequenceEqual(SHA256.HashData(replicaStream));
    }
}