using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FileManager.Collections;
using FileManager.Utils;

namespace FileManager.Models;

public class FileSystemInfoWrapper : INotifyPropertyChanged
{
    public FileSystemInfo FileSystemInfo { get; }
    private string m_FileSizeString = "Calculating...";

    public FileSystemInfoWrapper(FileSystemInfo fileSystemInfo)
    {
        FileSystemInfo = fileSystemInfo;
        Task.Run(() => FileSize = GetFileSize());
    }

    private long GetFileSize()
    {
        if (FileSystemInfo is FileInfo fileInfo)
        {
            return fileInfo.Length;
        }

        return ((DirectoryInfo)FileSystemInfo).GetFiles("*", new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true
            })
            .Sum(info => info.Length);
    }

    public string FileSizeString => FileSize.HasValue ? FileSize.Value.ToReadableByteString() : "Calculating...";


    private long? m_FileSize;

    public long? FileSize
    {
        get => m_FileSize;
        set
        {
            if (SetField(ref m_FileSize, value))
            {
                OnPropertyChanged(nameof(FileSizeString));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private FileSystemInfoChildren? m_Children;

    public FileSystemInfoChildren? Children => m_Children ??= FileSystemInfo is DirectoryInfo directoryInfo
        ? new FileSystemInfoChildren(directoryInfo.EnumerateFileSystemInfos("*",
            new EnumerationOptions()
            {
                IgnoreInaccessible = true, RecurseSubdirectories = false
            }).Select(i => new FileSystemInfoWrapper(i)))
        : null;
}