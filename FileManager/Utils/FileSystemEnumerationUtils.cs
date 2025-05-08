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

public static class FileSystemEnumerationUtils
{
    public static IEnumerable<FileSystemInfoWrapper> EnumerateFileSystemEntries(string fullPath,
        EnumerationOptions enumerationOptions)
    {
        FileSystemEnumerable<FileSystemInfoWrapper> enumerable = new FileSystemEnumerable<FileSystemInfoWrapper>(
            fullPath, (ref FileSystemEntry entry) => new FileSystemInfoWrapper(entry.ToFileSystemInfo()),
            enumerationOptions
        );
        return enumerable;
    }
}