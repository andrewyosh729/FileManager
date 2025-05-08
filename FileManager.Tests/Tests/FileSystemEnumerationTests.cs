using System.Diagnostics;
using FileManager.Models;
using FileManager.Utils;

namespace FileManager.Tests.Tests;

[TestFixture]
public class FileSystemEnumerationTests
{
    [Test]
    public void EnumerateCDriveParallel()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        DriveInfo driveInfo = new DriveInfo("C");
        HashSet<FileSystemInfoWrapper> fileSystemInfoWrappers = new();
        foreach (FileSystemInfoWrapper fileSystemInfoWrapper in FileSystemEnumerationUtils
                     .EnumerateFileSystemEntries(
                         driveInfo.RootDirectory.FullName,
                         new EnumerationOptions()
                         {
                             IgnoreInaccessible = true,
                             RecurseSubdirectories = true
                         }).AsParallel())
        {
            fileSystemInfoWrappers.Add(fileSystemInfoWrapper);
        }

        stopwatch.Stop();
        Console.WriteLine($"Seconds elapsed: {stopwatch.ElapsedMilliseconds / 1000f}");
    }

    [Test]
    public void EnumerateCDriveSynchronous()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        DriveInfo driveInfo = new DriveInfo("C");
        HashSet<FileSystemInfoWrapper> fileSystemInfoWrappers = new();
        foreach (FileSystemInfoWrapper fileSystemInfoWrapper in FileSystemEnumerationUtils
                     .EnumerateFileSystemEntries(
                         driveInfo.RootDirectory.FullName,
                         new EnumerationOptions()
                         {
                             IgnoreInaccessible = true,
                             RecurseSubdirectories = true
                         }))
        {
            fileSystemInfoWrappers.Add(fileSystemInfoWrapper);
        }

        stopwatch.Stop();
        Console.WriteLine($"Seconds elapsed: {stopwatch.ElapsedMilliseconds / 1000f}");
    }
}