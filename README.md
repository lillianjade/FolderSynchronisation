# FolderSyncApp

A C# console application that synchronises a source folder with a replica folder.

The application performs a one-way synchronisation from the source folder to the replica folder. It periodically checks for changes and ensures that the replica matches the source.

## Run the application

Open Command Prompt in the project folder and run:

`dotnet run -- "sourcePath" "replicaPath" intervalSeconds "logFilePath"`

Arguments:

* `sourcePath` – folder to synchronise
* `replicaPath` – folder where the copy is created
* `intervalSeconds` – synchronisation interval in seconds
* `logFilePath` – path to the log file

The application will continue synchronising at the specified interval.

## Stop the application
`Ctrl + C`
