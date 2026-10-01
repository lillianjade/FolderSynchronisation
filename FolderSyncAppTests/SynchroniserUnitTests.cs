using FolderSyncApp;
using Moq;
using NUnit.Framework;
using FolderSyncApp.Interfaces;

namespace FolderSyncAppTests;

[TestFixture]
public class SynchroniserTests
{
    private Mock<IFileSystem> _fileSystem = null!;
    private Mock<ILogger> _logger = null!;
    private Synchroniser _synchroniser = null!;

    private const string Source = @"C:\Source";
    private const string Replica = @"C:\Replica";

    [SetUp]
    public void Setup()
    {
        _fileSystem = new Mock<IFileSystem>();
        _logger = new Mock<ILogger>();

        _synchroniser = new Synchroniser(_fileSystem.Object, _logger.Object);
    }

    [Test]
    public void Sync_SourceDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        _fileSystem.Setup(x => x.DirectoryExists(Source)).Returns(false);

        // Act & Assert
        Assert.That(() => _synchroniser.Sync(Source, Replica),
            Throws.TypeOf<DirectoryNotFoundException>());
    }

    [Test]
    public void Sync_NewFile_CopiesFile()
    {
        // Arrange
        var sourceFile = Path.Combine(Source, "test.txt");
        var replicaFile = Path.Combine(Replica, "test.txt");

        SetupExistingSource();

        _fileSystem.Setup(x => x.GetFiles(Source)).Returns([sourceFile]);
        _fileSystem.Setup(x => x.FileExists(replicaFile)).Returns(false);

        // Act
        _synchroniser.Sync(Source, Replica);

        // Assert
        _fileSystem.Verify(x => x.CopyFile(sourceFile, replicaFile), Times.Once);
        _logger.Verify(x => x.Log("Copied file: test.txt"),Times.Once);
    }

    [Test]
    public void Sync_ChangedFile_UpdatesFile()
    {
        // Arrange
        var sourceFile = Path.Combine(Source, "test.txt");
        var replicaFile = Path.Combine(Replica, "test.txt");

        SetupExistingSource();

        _fileSystem.Setup(x => x.GetFiles(Source)).Returns([sourceFile]);
        _fileSystem.Setup(x => x.FileExists(replicaFile)).Returns(true);
        _fileSystem.Setup(x => x.FilesAreEqual(sourceFile, replicaFile)).Returns(false);

        // Act
        _synchroniser.Sync(Source, Replica);

        // Assert
        _fileSystem.Verify(x => x.CopyFile(sourceFile, replicaFile), Times.Once);
        _logger.Verify(x => x.Log("Updated file: test.txt"), Times.Once);
    }

    [Test]
    public void Sync_UnchangedFile_DoesNotCopyFile()
    {
        // Arrange
        var sourceFile = Path.Combine(Source, "test.txt");
        var replicaFile = Path.Combine(Replica, "test.txt");

        SetupExistingSource();

        _fileSystem.Setup(x => x.GetFiles(Source)).Returns([sourceFile]);
        _fileSystem.Setup(x => x.FileExists(replicaFile)).Returns(true);
        _fileSystem.Setup(x => x.FilesAreEqual(sourceFile, replicaFile)).Returns(true);

        // Act
        _synchroniser.Sync(Source, Replica);

        // Assert
        _fileSystem.Verify(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Sync_ObsoleteFile_DeletesFile()
    {
        // Arrange
        var replicaFile = Path.Combine(Replica, "obsolete.txt");

        SetupExistingSource();

        _fileSystem.Setup(x => x.GetFiles(Replica)).Returns([replicaFile]);
        _fileSystem.Setup(x => x.FileExists(Path.Combine(Source, "obsolete.txt"))).Returns(false);

        // Act
        _synchroniser.Sync(Source, Replica);

        // Assert
        _fileSystem.Verify(x => x.DeleteFile(replicaFile), Times.Once);
        _logger.Verify(x => x.Log("Removed file: obsolete.txt"), Times.Once);
    }

    [Test]
    public void Sync_ObsoleteDirectory_DeletesDirectory()
    {
        // Arrange
        var replicaDirectory = Path.Combine(Replica, "OldDirectory");

        SetupExistingSource();

        _fileSystem.Setup(x => x.GetDirectories(Replica)).Returns([replicaDirectory]);
        _fileSystem.Setup(x => x.DirectoryExists(Path.Combine(Source, "OldDirectory"))).Returns(false);

        // Act
        _synchroniser.Sync(Source, Replica);

        // Assert
        _fileSystem.Verify(x => x.DeleteDirectory(replicaDirectory, true), Times.Once);
        _logger.Verify(x => x.Log("Removed directory: OldDirectory"), Times.Once);
    }

    [Test]
    public void Sync_MissingDirectory_CreatesDirectory()
    {
        // Arrange
        var sourceDirectory = Path.Combine(Source, "Documents");
        var replicaDirectory = Path.Combine(Replica, "Documents");

        SetupExistingSource();

        _fileSystem.Setup(x => x.GetDirectories(Source)).Returns([sourceDirectory]);
        _fileSystem.Setup(x => x.DirectoryExists(replicaDirectory)).Returns(false);

        // Act
        _synchroniser.Sync(Source, Replica);

        // Assert
        _fileSystem.Verify(x => x.CreateDirectory(replicaDirectory), Times.Once);
        _logger.Verify(x => x.Log($"Created directory: {replicaDirectory}"), Times.Once);
    }

    private void SetupExistingSource()
    {
        _fileSystem.Setup(x => x.DirectoryExists(Source)).Returns(true);
    }
}