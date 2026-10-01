namespace FolderSyncApp.Interfaces;

public interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);

    IEnumerable<string> GetFiles(string path);
    IEnumerable<string> GetDirectories(string path);

    void CreateDirectory(string path);
    void CopyFile(string source, string destination);
    void DeleteFile(string path);
    void DeleteDirectory(string path, bool recursive);

    bool FilesAreEqual(string source, string replica);
}