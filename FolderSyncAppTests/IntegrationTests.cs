using FolderSyncApp;
using FolderSyncApp.Interfaces;
using NUnit.Framework;

namespace FolderSyncAppTests;

[TestFixture]
public class SynchroniserIntegrationTests
{
    private string _source = null!;
    private string _replica = null!;
    private Synchroniser _synchroniser = null!;

    [SetUp]
    public void Setup()
    {
        _source = Path.Combine(Path.GetTempPath(), "TestSourceDir");
        _replica = Path.Combine(Path.GetTempPath(), "TestReplicaDir");

        Directory.CreateDirectory(_source);

        var fileSystem = new FileSystem();
        var logger = new TestLogger();

        _synchroniser = new Synchroniser(fileSystem, logger);
    }

    [TearDown]
    public void Teardown()
    {
        if (Directory.Exists(_source))
            Directory.Delete(_source, true);

        if (Directory.Exists(_replica))
            Directory.Delete(_replica, true);
    }

    [Test]
    public void Sync_NewDirectoriesAndFiles_CreatesReplica()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_source, "Documents"));
        Directory.CreateDirectory(Path.Combine(_source, "Projects"));
        Directory.CreateDirectory(Path.Combine(_source, "Projects", "ProjectA"));

        File.WriteAllText(Path.Combine(_source, "root.txt"), "Root file");
        File.WriteAllText(Path.Combine(_source, "Documents", "document.txt"), "Document");
        File.WriteAllText(Path.Combine(_source, "Projects", "project.txt"), "Project");
        File.WriteAllText(Path.Combine(_source, "Projects", "ProjectA", "details.txt"), "Details");

        // Act
        _synchroniser.Sync(_source, _replica);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(_replica, "root.txt")), Is.True);
            Assert.That( File.Exists(Path.Combine(_replica, "Documents", "document.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(_replica, "Projects", "project.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(_replica, "Projects", "ProjectA", "details.txt")), Is.True);
        });
    }

    [Test]
    public void Sync_EmptySource_CreatesEmptyReplica()
    {
        // Act
        _synchroniser.Sync(_source, _replica);

        // Assert
        Assert.That(Directory.GetFileSystemEntries(_replica), Is.Empty);
    }

    [Test]
    public void Sync_ChangedFile_UpdatesReplica()
    {
        // Arrange
        Directory.CreateDirectory(_replica);

        var sourceFile = Path.Combine(_source, "test.txt");
        var replicaFile = Path.Combine(_replica, "test.txt");

        File.WriteAllText(sourceFile, "New content");
        File.WriteAllText(replicaFile, "Old content");

        // Act
        _synchroniser.Sync(_source, _replica);

        // Assert
        Assert.That(File.ReadAllText(replicaFile), Is.EqualTo("New content"));
    }

    [Test]
    public void Sync_ObsoleteFilesAndDirectories_RemovesThem()
    {
        // Arrange
        Directory.CreateDirectory(_replica);
        Directory.CreateDirectory(Path.Combine(_replica, "OldDirectory"));
        Directory.CreateDirectory(Path.Combine(_replica, "OldDirectory", "NestedDirectory"));

        File.WriteAllText(Path.Combine(_replica, "obsolete.txt"), "obsolete");
        File.WriteAllText(Path.Combine( _replica, "OldDirectory", "old.txt"), "obsolete");

        // Act
        _synchroniser.Sync(_source, _replica);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That( File.Exists(Path.Combine(_replica, "obsolete.txt")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(_replica, "OldDirectory")), Is.False);
        });
    }

    [Test]
    public void Sync_SameSizeDifferentContent_UpdatesReplica()
    {
        // Arrange
        Directory.CreateDirectory(_replica);

        var sourceFile = Path.Combine(_source, "test.txt");
        var replicaFile = Path.Combine(_replica, "test.txt");

        File.WriteAllText(sourceFile, "AAAA");
        File.WriteAllText(replicaFile, "BBBB");

        // Act
        _synchroniser.Sync(_source, _replica);

        // Assert
        Assert.That(File.ReadAllText(replicaFile), Is.EqualTo("AAAA"));
    }

    [Test]
    public void Sync_RepeatedSync_DoesNotChangeReplica()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_source, "test.txt"), "Hello");

        // Act
        _synchroniser.Sync(_source, _replica);
        _synchroniser.Sync(_source, _replica);

        // Assert
        Assert.That(File.ReadAllText(Path.Combine(_replica, "test.txt")), Is.EqualTo("Hello"));
    }

    public class TestLogger : ILogger
    {
        public void Log(string message)
        {
        }
    }
}