namespace FolderSyncApp;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine(
                "Usage: FolderSyncApp <sourcePath> <replicaPath> <intervalSeconds> <logFilePath>");
            return;
        }

        var sourcePath = args[0];
        var replicaPath = args[1];
        var intervalSeconds = int.Parse(args[2]);
        var logFilePath = args[3];

        var logger = new Logger(logFilePath);
        var fileSystem = new FileSystem();
        var synchroniser = new Synchroniser(fileSystem, logger);

        while (true)
        {
            synchroniser.Sync(sourcePath, replicaPath);
            Thread.Sleep(intervalSeconds * 1000);
        }
    }
}