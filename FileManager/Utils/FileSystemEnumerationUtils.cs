using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FileManager.Collections;
using FileManager.Models;

namespace FileManager.Utils;

public readonly struct FileSystemInfoLight
{
    public FileSystemInfoLight(DirectoryInfo directoryInfo)
    {
        Name = new ReadOnlyMemory<char>(directoryInfo.Name.ToCharArray());
        FileSizeBytes = null;
        LastWriteTime = directoryInfo.LastWriteTimeUtc.ToLocalTime();
        IsDirectory = true;
        FullName =  new ReadOnlyMemory<char>(directoryInfo.FullName.ToCharArray());;
    }

    public FileSystemInfoLight(FileSystemEntry entry)
    {
        Name = new ReadOnlyMemory<char>(entry.FileName.ToArray());
        FileSizeBytes = entry.Length;
        LastWriteTime = entry.LastWriteTimeUtc.LocalDateTime;
        IsDirectory = entry.IsDirectory;
        char[] separator = new char[] { '\\' };
        FullName = new ReadOnlyMemory<char>(entry.Directory.ToArray().Concat(separator).Concat(entry.FileName.ToArray()).ToArray());
    }

    public ReadOnlyMemory<char> Name { get; }
    public ReadOnlyMemory<char> FullName { get; }
    public long? FileSizeBytes { get; }
    public DateTimeOffset LastWriteTime { get; }
    public bool IsDirectory { get; }
}

public static class FileSystemEnumerationUtils
{
    public static IEnumerable<FileSystemInfoWrapper> EnumerateFileSystemEntries(string fullPath,
        EnumerationOptions enumerationOptions)
    {
        FileSystemEnumerable<FileSystemInfoWrapper> enumerable = new FileSystemEnumerable<FileSystemInfoWrapper>(
            fullPath, (ref FileSystemEntry entry) => new FileSystemInfoWrapper(new FileSystemInfoLight(entry)),
            enumerationOptions
        );
        return enumerable;
    }

    public static int SortComparison(object x, object y)
    {
        if (x is not FileSystemInfoWrapper f1 || y is not FileSystemInfoWrapper f2)
        {
            return 0;
        }

        if (f1.FileSize.HasValue && f2.FileSize.HasValue)
        {
            return f1.FileSize.Value.CompareTo(f2.FileSize.Value);
        }

        if (f1.FileSize.HasValue)
        {
            return 1;
        }

        if (f2.FileSize.HasValue)
        {
            return -1;
        }

        return 0;
    }
}